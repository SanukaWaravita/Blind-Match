using BlindMatchPAS.Helpers;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.ViewModels.Proposal;
using BlindMatchPAS.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = ApplicationRoles.Student)]
public class StudentController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IProposalService _proposalService;
    private readonly IUserManagementService _userManagementService;
    private readonly IResearchAreaService _researchAreaService;
    private readonly IMatchingService _matchingService;

    public StudentController(
        IDashboardService dashboardService,
        IProposalService proposalService,
        IUserManagementService userManagementService,
        IResearchAreaService researchAreaService,
        IMatchingService matchingService)
    {
        _dashboardService = dashboardService;
        _proposalService = proposalService;
        _userManagementService = userManagementService;
        _researchAreaService = researchAreaService;
        _matchingService = matchingService;
    }

    public async Task<IActionResult> Dashboard()
        => View(await _dashboardService.GetStudentDashboardAsync(User.GetUserId()!));

    public async Task<IActionResult> Proposals()
        => View(await _proposalService.GetStudentProposalsAsync(User.GetUserId()!));

    [HttpGet]
    public async Task<IActionResult> CreateProposal()
        => View(await BuildProposalFormAsync(new ProposalFormViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProposal(ProposalFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildProposalFormAsync(model));
        }

        await _proposalService.CreateAsync(User.GetUserId()!, model);
        TempData["SuccessMessage"] = "Proposal submitted successfully.";
        return RedirectToAction(nameof(Proposals));
    }

    [HttpGet]
    public async Task<IActionResult> EditProposal(int id)
    {
        var proposal = await _proposalService.GetByIdForStudentAsync(id, User.GetUserId()!);
        if (proposal is null)
        {
            return NotFound();
        }

        var model = new ProposalFormViewModel
        {
            Id = proposal.Id,
            Title = proposal.Title,
            Abstract = proposal.Abstract,
            TechnicalStack = proposal.TechnicalStack,
            ResearchAreaId = proposal.ResearchAreaId,
            AdditionalDescription = proposal.AdditionalDescription,
            Status = proposal.Status
        };

        return View(await BuildProposalFormAsync(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProposal(int id, ProposalFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildProposalFormAsync(model));
        }

        await _proposalService.UpdateAsync(id, User.GetUserId()!, model);
        TempData["SuccessMessage"] = "Proposal updated.";
        return RedirectToAction(nameof(ProposalDetails), new { id });
    }

    public async Task<IActionResult> ProposalDetails(int id)
    {
        var proposal = await _proposalService.GetByIdForStudentAsync(id, User.GetUserId()!);
        if (proposal is null)
        {
            return NotFound();
        }

        ViewBag.ActiveMatch = await _matchingService.GetActiveMatchByProposalIdAsync(id);
        return View(proposal);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WithdrawProposal(int id)
    {
        await _proposalService.WithdrawAsync(id, User.GetUserId()!);
        TempData["SuccessMessage"] = "Proposal withdrawn.";
        return RedirectToAction(nameof(Proposals));
    }

    public async Task<IActionResult> TrackStatus()
        => View(await _proposalService.GetStudentProposalsAsync(User.GetUserId()!));

    public async Task<IActionResult> MatchedSupervisor(int id)
    {
        var proposal = await _proposalService.GetByIdForStudentAsync(id, User.GetUserId()!);
        if (proposal is null || !proposal.IsMatched)
        {
            return NotFound();
        }

        ViewBag.ActiveMatch = await _matchingService.GetActiveMatchByProposalIdAsync(id);
        return View(proposal);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var profile = await _userManagementService.GetStudentProfileAsync(User.GetUserId()!);
        if (profile is null)
        {
            return NotFound();
        }

        return View(new StudentProfileViewModel
        {
            FullName = profile.User.FullName,
            Email = profile.User.Email ?? string.Empty,
            StudentIdNumber = profile.StudentIdNumber,
            Department = profile.Department,
            ContactNumber = profile.ContactNumber,
            ProjectOwnershipType = profile.ProjectOwnershipType
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(StudentProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _userManagementService.UpdateStudentProfileAsync(User.GetUserId()!, model);
        TempData["SuccessMessage"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    private async Task<ProposalFormViewModel> BuildProposalFormAsync(ProposalFormViewModel model)
    {
        var areas = await _researchAreaService.GetAllAsync();
        model.ResearchAreas = areas.Select(x => new SelectListItem(x.Name, x.Id.ToString()));
        return model;
    }
}
