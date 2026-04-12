using BlindMatchPAS.Data;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public class MatchingService : IMatchingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public MatchingService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task ExpressInterestAsync(int proposalId, string supervisorUserId)
    {
        var supervisor = await _context.SupervisorProfiles
            .FirstAsync(x => x.UserId == supervisorUserId);

        var proposal = await _context.ProjectProposals.FirstAsync(x => x.Id == proposalId);
        EnsureAvailableForMatching(proposal);

        var existing = await _context.InterestRecords
            .FirstOrDefaultAsync(x => x.ProposalId == proposalId && x.SupervisorProfileId == supervisor.Id);

        if (existing is null)
        {
            await _context.InterestRecords.AddAsync(new InterestRecord
            {
                ProposalId = proposalId,
                SupervisorProfileId = supervisor.Id,
                Status = InterestStatus.Active
            });
        }
        else
        {
            existing.Status = InterestStatus.Active;
            existing.ExpressedAt = DateTime.UtcNow;
        }

        proposal.Status = proposal.Status == ProposalStatus.Pending ? ProposalStatus.UnderReview : ProposalStatus.Interested;
        proposal.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("InterestExpressed", nameof(ProjectProposal), proposalId.ToString(), $"SupervisorProfileId: {supervisor.Id}", supervisorUserId);
    }

    public async Task ConfirmMatchAsync(int proposalId, string supervisorUserId, string? notes = null)
    {
        var supervisor = await _context.SupervisorProfiles.FirstAsync(x => x.UserId == supervisorUserId);
        var proposal = await _context.ProjectProposals
            .Include(x => x.InterestRecords)
            .Include(x => x.MatchRecords)
            .FirstAsync(x => x.Id == proposalId);

        EnsureAvailableForMatching(proposal);

        var interest = proposal.InterestRecords.FirstOrDefault(x =>
            x.SupervisorProfileId == supervisor.Id && x.Status == InterestStatus.Active);

        if (interest is null)
        {
            throw new InvalidOperationException("Supervisor must express interest before confirming a match.");
        }

        interest.Status = InterestStatus.Confirmed;

        foreach (var otherInterest in proposal.InterestRecords.Where(x => x.Id != interest.Id && x.Status == InterestStatus.Active))
        {
            otherInterest.Status = InterestStatus.Rejected;
        }

        foreach (var oldMatch in proposal.MatchRecords.Where(x => x.IsActive))
        {
            oldMatch.IsActive = false;
        }

        var matchedAt = DateTime.UtcNow;
        await _context.MatchRecords.AddAsync(new MatchRecord
        {
            ProposalId = proposal.Id,
            SupervisorProfileId = supervisor.Id,
            ConfirmedBySupervisor = true,
            MatchedAt = matchedAt,
            RevealedAt = matchedAt,
            Notes = notes,
            IsActive = true
        });

        proposal.Status = ProposalStatus.Matched;
        proposal.IsMatched = true;
        proposal.MatchedAt = matchedAt;
        proposal.UpdatedAt = matchedAt;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("MatchConfirmed", nameof(ProjectProposal), proposal.Id.ToString(), $"SupervisorProfileId: {supervisor.Id}", supervisorUserId);
    }

    public async Task ManualAssignAsync(int proposalId, int supervisorProfileId, string adminUserId, string? notes = null)
    {
        var proposal = await _context.ProjectProposals
            .Include(x => x.MatchRecords)
            .FirstAsync(x => x.Id == proposalId);

        if (proposal.IsWithdrawn)
        {
            throw new InvalidOperationException("Withdrawn proposals cannot be reassigned.");
        }

        foreach (var oldMatch in proposal.MatchRecords.Where(x => x.IsActive))
        {
            oldMatch.IsActive = false;
        }

        var timestamp = DateTime.UtcNow;
        proposal.IsMatched = true;
        proposal.Status = proposal.MatchRecords.Any() ? ProposalStatus.Reassigned : ProposalStatus.Matched;
        proposal.MatchedAt = timestamp;
        proposal.UpdatedAt = timestamp;

        await _context.MatchRecords.AddAsync(new MatchRecord
        {
            ProposalId = proposalId,
            SupervisorProfileId = supervisorProfileId,
            ConfirmedBySupervisor = true,
            MatchedAt = timestamp,
            RevealedAt = timestamp,
            IsActive = true,
            Notes = notes
        });

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("ManualAssignment", nameof(ProjectProposal), proposalId.ToString(), $"SupervisorProfileId: {supervisorProfileId}", adminUserId);
    }

    public async Task<List<InterestRecord>> GetSupervisorInterestsAsync(string supervisorUserId)
    {
        var supervisorId = await _context.SupervisorProfiles
            .Where(x => x.UserId == supervisorUserId)
            .Select(x => x.Id)
            .FirstAsync();

        return await _context.InterestRecords
            .Include(x => x.Proposal).ThenInclude(x => x.ResearchArea)
            .Where(x => x.SupervisorProfileId == supervisorId)
            .OrderByDescending(x => x.ExpressedAt)
            .ToListAsync();
    }

    public async Task<List<MatchRecord>> GetSupervisorMatchesAsync(string supervisorUserId)
    {
        var supervisorId = await _context.SupervisorProfiles
            .Where(x => x.UserId == supervisorUserId)
            .Select(x => x.Id)
            .FirstAsync();

        return await _context.MatchRecords
            .Include(x => x.Proposal).ThenInclude(x => x.ResearchArea)
            .Include(x => x.Proposal).ThenInclude(x => x.StudentProfile).ThenInclude(x => x.User)
            .Where(x => x.SupervisorProfileId == supervisorId && x.IsActive)
            .OrderByDescending(x => x.MatchedAt)
            .ToListAsync();
    }

    public Task<MatchRecord?> GetActiveMatchByProposalIdAsync(int proposalId)
        => _context.MatchRecords
            .Include(x => x.SupervisorProfile).ThenInclude(x => x.User)
            .Include(x => x.Proposal).ThenInclude(x => x.StudentProfile).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.ProposalId == proposalId && x.IsActive);

    private static void EnsureAvailableForMatching(ProjectProposal proposal)
    {
        if (proposal.IsWithdrawn || proposal.Status == ProposalStatus.Withdrawn)
        {
            throw new InvalidOperationException("Withdrawn proposals cannot be matched.");
        }

        if (proposal.IsMatched || proposal.Status == ProposalStatus.Matched)
        {
            throw new InvalidOperationException("Proposal is already matched.");
        }
    }
}
