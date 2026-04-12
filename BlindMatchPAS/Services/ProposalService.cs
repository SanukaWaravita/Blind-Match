using BlindMatchPAS.Data;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.ViewModels.Proposal;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public class ProposalService : IProposalService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public ProposalService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<int> CreateAsync(string studentUserId, ProposalFormViewModel model)
    {
        var studentProfile = await _context.StudentProfiles.FirstAsync(x => x.UserId == studentUserId);
        var proposal = new ProjectProposal
        {
            Title = model.Title.Trim(),
            Abstract = model.Abstract.Trim(),
            TechnicalStack = model.TechnicalStack.Trim(),
            ResearchAreaId = model.ResearchAreaId,
            StudentProfileId = studentProfile.Id,
            AdditionalDescription = model.AdditionalDescription?.Trim()
        };

        await _context.ProjectProposals.AddAsync(proposal);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("ProposalCreated", nameof(ProjectProposal), proposal.Id.ToString(), proposal.Title, studentUserId);
        return proposal.Id;
    }

    public async Task UpdateAsync(int proposalId, string studentUserId, ProposalFormViewModel model)
    {
        var proposal = await GetByIdForStudentAsync(proposalId, studentUserId)
                       ?? throw new InvalidOperationException("Proposal not found.");
        EnsureModifiable(proposal);
        proposal.Title = model.Title.Trim();
        proposal.Abstract = model.Abstract.Trim();
        proposal.TechnicalStack = model.TechnicalStack.Trim();
        proposal.ResearchAreaId = model.ResearchAreaId;
        proposal.AdditionalDescription = model.AdditionalDescription?.Trim();
        proposal.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("ProposalUpdated", nameof(ProjectProposal), proposal.Id.ToString(), proposal.Title, studentUserId);
    }

    public async Task WithdrawAsync(int proposalId, string studentUserId)
    {
        var proposal = await GetByIdForStudentAsync(proposalId, studentUserId)
                       ?? throw new InvalidOperationException("Proposal not found.");
        EnsureModifiable(proposal);
        proposal.IsWithdrawn = true;
        proposal.Status = ProposalStatus.Withdrawn;
        proposal.WithdrawnAt = DateTime.UtcNow;
        proposal.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("ProposalWithdrawn", nameof(ProjectProposal), proposal.Id.ToString(), proposal.Title, studentUserId);
    }

    public Task<ProjectProposal?> GetByIdAsync(int id)
        => _context.ProjectProposals
            .Include(x => x.ResearchArea)
            .Include(x => x.StudentProfile).ThenInclude(x => x.User)
            .Include(x => x.MatchRecords).ThenInclude(x => x.SupervisorProfile).ThenInclude(x => x.User)
            .Include(x => x.InterestRecords)
            .FirstOrDefaultAsync(x => x.Id == id);

    public Task<ProjectProposal?> GetByIdForStudentAsync(int id, string studentUserId)
        => _context.ProjectProposals
            .Include(x => x.ResearchArea)
            .Include(x => x.StudentProfile).ThenInclude(x => x.User)
            .Include(x => x.MatchRecords).ThenInclude(x => x.SupervisorProfile).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id && x.StudentProfile.UserId == studentUserId);

    public Task<List<ProjectProposal>> GetStudentProposalsAsync(string studentUserId)
        => _context.ProjectProposals
            .Include(x => x.ResearchArea)
            .Include(x => x.MatchRecords.Where(m => m.IsActive)).ThenInclude(x => x.SupervisorProfile).ThenInclude(x => x.User)
            .Where(x => x.StudentProfile.UserId == studentUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<List<ProjectProposal>> GetAdminProposalsAsync(ProposalFilterViewModel filter)
    {
        var query = _context.ProjectProposals
            .Include(x => x.ResearchArea)
            .Include(x => x.StudentProfile).ThenInclude(x => x.User)
            .Include(x => x.MatchRecords.Where(m => m.IsActive)).ThenInclude(x => x.SupervisorProfile).ThenInclude(x => x.User)
            .AsQueryable();

        query = ApplyFilter(query, filter);
        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<List<ProjectProposal>> GetAnonymousProposalsForSupervisorAsync(string supervisorUserId, ProposalFilterViewModel filter)
    {
        _ = await _context.SupervisorProfiles.FirstAsync(x => x.UserId == supervisorUserId);

        var query = _context.ProjectProposals
            .Include(x => x.ResearchArea)
            .Include(x => x.StudentProfile).ThenInclude(x => x.User)
            .Include(x => x.InterestRecords)
            .Where(x => !x.IsWithdrawn && !x.IsMatched)
            .AsQueryable();

        query = ApplyFilter(query, filter);
        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<bool> CanStudentModifyAsync(int proposalId, string studentUserId)
    {
        var proposal = await GetByIdForStudentAsync(proposalId, studentUserId);
        return proposal is not null && !proposal.IsMatched && !proposal.IsWithdrawn;
    }

    private static IQueryable<ProjectProposal> ApplyFilter(IQueryable<ProjectProposal> query, ProposalFilterViewModel filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            query = query.Where(x => x.Title.Contains(filter.Keyword) || x.Abstract.Contains(filter.Keyword));
        }

        if (filter.ResearchAreaId.HasValue)
        {
            query = query.Where(x => x.ResearchAreaId == filter.ResearchAreaId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.TechnicalStack))
        {
            query = query.Where(x => x.TechnicalStack.Contains(filter.TechnicalStack));
        }

        return query;
    }

    private static void EnsureModifiable(ProjectProposal proposal)
    {
        if (proposal.IsMatched || proposal.Status == ProposalStatus.Matched)
        {
            throw new InvalidOperationException("Matched proposals can no longer be edited.");
        }

        if (proposal.IsWithdrawn || proposal.Status == ProposalStatus.Withdrawn)
        {
            throw new InvalidOperationException("Withdrawn proposals cannot be changed.");
        }
    }
}
