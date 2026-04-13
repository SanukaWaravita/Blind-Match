using BlindMatchPAS.Data;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.ViewModels.Admin;
using BlindMatchPAS.ViewModels.Proposal;
using BlindMatchPAS.ViewModels.Student;
using BlindMatchPAS.ViewModels.Supervisor;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IProposalService _proposalService;

    public DashboardService(ApplicationDbContext context, IProposalService proposalService)
    {
        _context = context;
        _proposalService = proposalService;
    }

    public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(string userId)
    {
        var proposals = await _proposalService.GetStudentProposalsAsync(userId);

        var studentName = await _context.StudentProfiles
            .Where(x => x.UserId == userId)
            .Select(x => x.User.FullName)
            .FirstAsync();

        return new StudentDashboardViewModel
        {
            StudentName = studentName,
            ProposalCount = proposals.Count,
            PendingCount = proposals.Count(IsInStudentPipeline),
            MatchedCount = proposals.Count(HasRevealedSupervisor),
            Proposals = proposals.Select(MapProposal).ToList()
        };
    }

    public async Task<SupervisorDashboardViewModel> GetSupervisorDashboardAsync(string userId)
    {
        var supervisorId = await _context.SupervisorProfiles.Where(x => x.UserId == userId).Select(x => x.Id).FirstAsync();
        var anonymous = await _context.ProjectProposals
            .Include(x => x.ResearchArea)
            .Where(x => !x.IsMatched && !x.IsWithdrawn)
            .OrderByDescending(x => x.CreatedAt)
            .Take(6)
            .ToListAsync();

        var name = await _context.SupervisorProfiles.Where(x => x.UserId == userId).Select(x => x.User.FullName).FirstAsync();

        return new SupervisorDashboardViewModel
        {
            SupervisorName = name,
            AnonymousProposalCount = anonymous.Count,
            ActiveInterestCount = await _context.InterestRecords.CountAsync(x => x.SupervisorProfileId == supervisorId && x.Status == InterestStatus.Active),
            MatchCount = await _context.MatchRecords.CountAsync(x => x.SupervisorProfileId == supervisorId && x.IsActive),
            AnonymousProposals = anonymous.Select(MapProposal).Select(x => { x.IsAnonymous = true; return x; }).ToList()
        };
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
    {
        return new AdminDashboardViewModel
        {
            TotalStudents = await _context.StudentProfiles.CountAsync(),
            TotalSupervisors = await _context.SupervisorProfiles.CountAsync(),
            TotalProposals = await _context.ProjectProposals.CountAsync(),
            PendingProposals = await _context.ProjectProposals.CountAsync(x => x.Status == ProposalStatus.Pending),
            MatchedProposals = await _context.ProjectProposals.CountAsync(x => x.Status == ProposalStatus.Matched || x.Status == ProposalStatus.Reassigned),
            WithdrawnProposals = await _context.ProjectProposals.CountAsync(x => x.Status == ProposalStatus.Withdrawn),
            RecentMatches = await _context.MatchRecords
                .Include(x => x.Proposal).ThenInclude(x => x.StudentProfile).ThenInclude(x => x.User)
                .Include(x => x.SupervisorProfile).ThenInclude(x => x.User)
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.MatchedAt)
                .Take(5)
                .Select(x => new RecentMatchViewModel
                {
                    ProposalTitle = x.Proposal.Title,
                    StudentName = x.Proposal.StudentProfile.User.FullName,
                    SupervisorName = x.SupervisorProfile.User.FullName,
                    MatchedAt = x.MatchedAt
                }).ToListAsync(),
            RecentActivity = await _context.AuditLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(6)
                .Select(x => new RecentActivityViewModel
                {
                    Action = x.Action,
                    Details = x.Details ?? $"{x.EntityName} #{x.EntityId}",
                    Timestamp = x.Timestamp
                }).ToListAsync()
        };
    }

    public async Task<ReportsViewModel> GetReportsAsync()
    {
        return new ReportsViewModel
        {
            ProposalStatusSummary = await _context.ProjectProposals
                .GroupBy(x => x.Status)
                .Select(g => new StatusSummaryViewModel { Status = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            ProposalsByArea = await _context.ProjectProposals
                .GroupBy(x => x.ResearchArea.Name)
                .Select(g => new AreaSummaryViewModel { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            SupervisorsByDepartment = await _context.SupervisorProfiles
                .GroupBy(x => x.Department)
                .Select(g => new DepartmentSummaryViewModel { Department = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync(),
            RecentMatches = await _context.MatchRecords
                .Include(x => x.Proposal).ThenInclude(x => x.StudentProfile).ThenInclude(x => x.User)
                .Include(x => x.SupervisorProfile).ThenInclude(x => x.User)
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.MatchedAt)
                .Take(10)
                .Select(x => new RecentMatchViewModel
                {
                    ProposalTitle = x.Proposal.Title,
                    StudentName = x.Proposal.StudentProfile.User.FullName,
                    SupervisorName = x.SupervisorProfile.User.FullName,
                    MatchedAt = x.MatchedAt
                }).ToListAsync()
        };
    }

    private static ProposalListItemViewModel MapProposal(Models.ProjectProposal proposal)
    {
        var activeMatch = proposal.MatchRecords.FirstOrDefault(x => x.IsActive);
        return new ProposalListItemViewModel
        {
            Id = proposal.Id,
            Title = proposal.Title,
            Abstract = proposal.Abstract,
            TechnicalStack = proposal.TechnicalStack,
            ResearchArea = proposal.ResearchArea.Name,
            Status = proposal.Status,
            CreatedAt = proposal.CreatedAt,
            IsMatched = proposal.IsMatched,
            StudentName = proposal.StudentProfile?.User?.FullName,
            StudentEmail = proposal.StudentProfile?.User?.Email,
            SupervisorName = activeMatch?.SupervisorProfile?.User?.FullName
        };
    }

    private static bool IsInStudentPipeline(ProjectProposal proposal)
        => !proposal.IsWithdrawn &&
           !proposal.IsMatched &&
           proposal.Status is ProposalStatus.Pending or ProposalStatus.UnderReview or ProposalStatus.Interested;

    private static bool HasRevealedSupervisor(ProjectProposal proposal)
        => proposal.MatchRecords.Any(x => x.IsActive && x.RevealedAt.HasValue);
}
