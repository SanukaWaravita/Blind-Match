using BlindMatchPAS.Data;
using BlindMatchPAS.Helpers;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.ViewModels.Admin;
using BlindMatchPAS.ViewModels.Proposal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IUserManagementService _userManagementService;
    private readonly IResearchAreaService _researchAreaService;
    private readonly IProposalService _proposalService;
    private readonly IMatchingService _matchingService;
    private readonly IAuditService _auditService;
    private readonly ApplicationDbContext _context;

    public AdminController(
        IDashboardService dashboardService,
        IUserManagementService userManagementService,
        IResearchAreaService researchAreaService,
        IProposalService proposalService,
        IMatchingService matchingService,
        IAuditService auditService,
        ApplicationDbContext context)
    {
        _dashboardService = dashboardService;
        _userManagementService = userManagementService;
        _researchAreaService = researchAreaService;
        _proposalService = proposalService;
        _matchingService = matchingService;
        _auditService = auditService;
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
        => View(await _dashboardService.GetAdminDashboardAsync());

    public async Task<IActionResult> Users(string? keyword, string? role)
    {
        ViewBag.Keyword = keyword;
        ViewBag.Role = role;
        return View(await _userManagementService.GetUsersAsync(keyword, role));
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var model = await _userManagementService.GetEditUserAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(AdminEditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _userManagementService.UpdateUserAsync(model);
        TempData["SuccessMessage"] = "User updated.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public IActionResult CreateUser()
        => View(new AdminCreateUserViewModel { Role = ApplicationRoles.Student });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _userManagementService.CreateUserAsync(model);
            TempData["SuccessMessage"] = "User account created.";
            return RedirectToAction(nameof(Users));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserActive(string id)
    {
        await _userManagementService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> ResearchAreas()
        => View(await _researchAreaService.GetAllAsync(includeInactive: true));

    [HttpGet]
    public IActionResult CreateResearchArea() => View(new ResearchAreaFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResearchArea(ResearchAreaFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _researchAreaService.CreateAsync(model);
        TempData["SuccessMessage"] = "Research area created.";
        return RedirectToAction(nameof(ResearchAreas));
    }

    [HttpGet]
    public async Task<IActionResult> EditResearchArea(int id)
    {
        var area = await _researchAreaService.GetByIdAsync(id);
        if (area is null)
        {
            return NotFound();
        }

        return View(new ResearchAreaFormViewModel
        {
            Id = area.Id,
            Name = area.Name,
            Description = area.Description,
            IsActive = area.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResearchArea(ResearchAreaFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _researchAreaService.UpdateAsync(model);
        TempData["SuccessMessage"] = "Research area updated.";
        return RedirectToAction(nameof(ResearchAreas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleResearchArea(int id)
    {
        await _researchAreaService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(ResearchAreas));
    }

    public async Task<IActionResult> Proposals([FromQuery] ProposalFilterViewModel filter)
    {
        filter.ResearchAreas = (await _researchAreaService.GetAllAsync(includeInactive: true))
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()));
        ViewBag.Filter = filter;
        return View(await _proposalService.GetAdminProposalsAsync(filter));
    }

    public async Task<IActionResult> Matches()
    {
        var matches = await _context.MatchRecords
            .Include(x => x.Proposal).ThenInclude(x => x.StudentProfile).ThenInclude(x => x.User)
            .Include(x => x.SupervisorProfile).ThenInclude(x => x.User)
            .OrderByDescending(x => x.MatchedAt)
            .Select(x => new AdminMatchViewModel
            {
                ProposalId = x.ProposalId,
                ProposalTitle = x.Proposal.Title,
                StudentName = x.Proposal.StudentProfile.User.FullName,
                SupervisorName = x.SupervisorProfile.User.FullName,
                MatchedAt = x.MatchedAt,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return View(matches);
    }

    [HttpGet]
    public async Task<IActionResult> ManualAssign(int proposalId)
    {
        var proposal = await _proposalService.GetByIdAsync(proposalId);
        if (proposal is null)
        {
            return NotFound();
        }

        var supervisors = await _context.SupervisorProfiles
            .Include(x => x.User)
            .Include(x => x.ResearchAreas)
            .Where(x => x.User.IsActive)
            .OrderBy(x => x.User.FullName)
            .ToListAsync();

        return View(new ManualAssignmentViewModel
        {
            ProposalId = proposalId,
            ProposalTitle = proposal.Title,
            Supervisors = supervisors.Select(x =>
            {
                var prefix = x.ResearchAreas.Any(r => r.ResearchAreaId == proposal.ResearchAreaId) ? "[Aligned]" : "[Manual]";
                return new SelectListItem($"{prefix} {x.User.FullName} - {x.Department}", x.Id.ToString());
            })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManualAssign(ManualAssignmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Supervisors = (await _context.SupervisorProfiles
                .Include(x => x.User)
                .OrderBy(x => x.User.FullName)
                .ToListAsync())
                .Select(x => new SelectListItem($"{x.User.FullName} - {x.Department}", x.Id.ToString()));
            return View(model);
        }

        await _matchingService.ManualAssignAsync(model.ProposalId, model.SupervisorProfileId, User.GetUserId()!, model.Notes);
        TempData["SuccessMessage"] = "Manual assignment completed.";
        return RedirectToAction(nameof(Matches));
    }

    public async Task<IActionResult> Reports()
        => View(await _dashboardService.GetReportsAsync());

    public async Task<FileResult> ExportMatchesCsv()
    {
        var lines = new List<string> { "ProposalTitle,StudentName,SupervisorName,MatchedAt,IsActive" };
        var matches = await _context.MatchRecords
            .Include(x => x.Proposal).ThenInclude(x => x.StudentProfile).ThenInclude(x => x.User)
            .Include(x => x.SupervisorProfile).ThenInclude(x => x.User)
            .OrderByDescending(x => x.MatchedAt)
            .ToListAsync();

        lines.AddRange(matches.Select(x =>
            $"\"{x.Proposal.Title}\",\"{x.Proposal.StudentProfile.User.FullName}\",\"{x.SupervisorProfile.User.FullName}\",\"{x.MatchedAt:u}\",\"{x.IsActive}\""));

        return File(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)), "text/csv", "blind-match-allocations.csv");
    }

    public async Task<IActionResult> AuditLog()
        => View(await _auditService.GetRecentAsync(100));
}
