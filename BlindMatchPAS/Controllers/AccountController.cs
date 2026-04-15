using BlindMatchPAS.Helpers;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserManagementService _userManagementService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserManagementService userManagementService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid account details.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Supervisor))
        {
            return RedirectToAction("Dashboard", "Supervisor");
        }

        return RedirectToAction("Dashboard", "Student");
    }

    [HttpGet]
    public IActionResult Register()
        => View(new RegisterViewModel { Role = ApplicationRoles.Student });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            if (model.Role == ApplicationRoles.Supervisor)
            {
                await _userManagementService.RegisterSupervisorAsync(model);
            }
            else
            {
                await _userManagementService.RegisterStudentAsync(model);
            }

            TempData["SuccessMessage"] = "Your account has been created. Please sign in.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
