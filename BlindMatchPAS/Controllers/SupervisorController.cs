using BlindMatchPAS.Helpers;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.ViewModels.Proposal;
using BlindMatchPAS.ViewModels.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = ApplicationRoles.Supervisor)]
public class SupervisorController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IProposalService _proposalService;
    private readonly IMatchingService _matchingService;
    private readonly IUserManagementService _userManagementService;
    private readonly IResearchAreaService _researchAreaService;

    public SupervisorController(
        IDashboardService dashboardService,
        IProposalService proposalService,
        IMatchingService matchingService,
        IUserManagementService userManagementService,
        IResearchAreaService researchAreaService)
    {
        _dashboardService = dashboardService;
        _proposalService = proposalService;
        _matchingService = matchingService;
        _userManagementService = userManagementService;
        _researchAreaService = researchAreaService;
    }

    public async Task<IActionResult> Dashboard()
        => View(await _dashboardService.GetSupervisorDashboardAsync(User.GetUserId()!));

    public async Task<IActionResult> BrowseProposals([FromQuery] ProposalFilterViewModel filter)
    {
        filter.ResearchAreas = (await _researchAreaService.GetAllAsync())
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()));
        ViewBag.Filter = filter;
        return View(await _proposalService.GetAnonymousProposalsForSupervisorAsync(User.GetUserId()!, filter));
    }

    public async Task<IActionResult> ProposalDetails(int id)
    {
        var proposal = await _proposalService.GetByIdAsync(id);
        if (proposal is null)
        {
            return NotFound();
        }

        var activeMatch = await _matchingService.GetActiveMatchByProposalIdAsync(id);
        var isRevealed = activeMatch?.SupervisorProfile.UserId == User.GetUserId();
        ViewBag.IsRevealed = isRevealed;
        ViewBag.ActiveMatch = activeMatch;
        return View(proposal);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExpressInterest(int id)
    {
        await _matchingService.ExpressInterestAsync(id, User.GetUserId()!);
        TempData["SuccessMessage"] = "Interest recorded. The proposal remains anonymous until confirmation.";
        return RedirectToAction(nameof(InterestedProposals));
    }

    public async Task<IActionResult> InterestedProposals()
        => View(await _matchingService.GetSupervisorInterestsAsync(User.GetUserId()!));

    [HttpGet]
    public async Task<IActionResult> ConfirmMatch(int id)
    {
        var proposal = await _proposalService.GetByIdAsync(id);
        if (proposal is null)
        {
            return NotFound();
        }

        return View(proposal);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmMatch(int id, string? notes)
    {
        await _matchingService.ConfirmMatchAsync(id, User.GetUserId()!, notes);
        TempData["SuccessMessage"] = "Match confirmed. Student identity is now revealed for this allocation.";
        return RedirectToAction(nameof(MatchedStudents));
    }

    public async Task<IActionResult> MatchedStudents()
        => View(await _matchingService.GetSupervisorMatchesAsync(User.GetUserId()!));

    [HttpGet]
    public async Task<IActionResult> Expertise()
    {
        var profile = await _userManagementService.GetSupervisorProfileAsync(User.GetUserId()!);
        if (profile is null)
        {
            return NotFound();
        }

        var areas = await _researchAreaService.GetAllAsync();
        ViewBag.ResearchAreas = areas.Select(x => new SelectListItem(x.Name, x.Id.ToString()));
        return View(new SupervisorProfileViewModel
        {
            FullName = profile.User.FullName,
            Email = profile.User.Email ?? string.Empty,
            EmployeeId = profile.EmployeeId,
            Department = profile.Department,
            ContactNumber = profile.ContactNumber,
            Bio = profile.Bio,
            SelectedResearchAreaIds = profile.ResearchAreas.Select(x => x.ResearchAreaId).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Expertise(SupervisorProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ResearchAreas = (await _researchAreaService.GetAllAsync()).Select(x => new SelectListItem(x.Name, x.Id.ToString()));
            return View(model);
        }

        await _userManagementService.UpdateSupervisorProfileAsync(User.GetUserId()!, model);
        TempData["SuccessMessage"] = "Supervisor profile and expertise updated.";
        return RedirectToAction(nameof(Expertise));
    }

    public Task<IActionResult> Profile() => Expertise();
}
