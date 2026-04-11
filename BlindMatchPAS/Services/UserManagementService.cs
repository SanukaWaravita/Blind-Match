using BlindMatchPAS.Data;
using BlindMatchPAS.Helpers;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels.Account;
using BlindMatchPAS.ViewModels.Admin;
using BlindMatchPAS.ViewModels.Student;
using BlindMatchPAS.ViewModels.Supervisor;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public UserManagementService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IAuditService auditService)
    {
        _userManager = userManager;
        _context = context;
        _auditService = auditService;
    }

    public Task RegisterStudentAsync(RegisterViewModel model) => CreateInternalAsync(model, ApplicationRoles.Student);

    public Task RegisterSupervisorAsync(RegisterViewModel model) => CreateInternalAsync(model, ApplicationRoles.Supervisor);

    public async Task<List<UserManagementItemViewModel>> GetUsersAsync(string? keyword, string? role)
    {
        var users = await _userManager.Users
            .Include(x => x.StudentProfile)
            .Include(x => x.SupervisorProfile)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var items = new List<UserManagementItemViewModel>();
        foreach (var user in users)
        {
            var selectedRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "-";
            if (!string.IsNullOrWhiteSpace(role) && !selectedRole.Equals(role, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var department = user.StudentProfile?.Department ?? user.SupervisorProfile?.Department ?? "-";
            if (!string.IsNullOrWhiteSpace(keyword) &&
                !($"{user.FullName} {user.Email} {department}".Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            items.Add(new UserManagementItemViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = selectedRole,
                IsActive = user.IsActive,
                Department = department,
                CreatedAt = user.CreatedAt
            });
        }

        return items;
    }

    public async Task ToggleActiveAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        await _auditService.LogAsync("UserStatusChanged", nameof(ApplicationUser), user.Id, $"IsActive: {user.IsActive}");
    }

    public async Task CreateUserAsync(AdminCreateUserViewModel model)
    {
        if (model.Role == ApplicationRoles.Admin)
        {
            await CreateInternalAsync(model, ApplicationRoles.Admin);
            return;
        }

        if (model.Role == ApplicationRoles.Supervisor)
        {
            await RegisterSupervisorAsync(model);
            return;
        }

        await RegisterStudentAsync(model);
    }

    public async Task<AdminEditUserViewModel?> GetEditUserAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(x => x.StudentProfile)
            .Include(x => x.SupervisorProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            return null;
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? ApplicationRoles.Student;
        return new AdminEditUserViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = role,
            StudentIdNumber = user.StudentProfile?.StudentIdNumber,
            EmployeeId = user.SupervisorProfile?.EmployeeId,
            Department = user.StudentProfile?.Department ?? user.SupervisorProfile?.Department ?? string.Empty,
            ContactNumber = user.StudentProfile?.ContactNumber ?? user.SupervisorProfile?.ContactNumber ?? string.Empty,
            ProjectOwnershipType = user.StudentProfile?.ProjectOwnershipType ?? Models.Enums.ProjectOwnershipType.Individual,
            Bio = user.SupervisorProfile?.Bio,
            IsActive = user.IsActive
        };
    }

    public async Task UpdateUserAsync(AdminEditUserViewModel model)
    {
        var user = await _userManager.Users
            .Include(x => x.StudentProfile)
            .Include(x => x.SupervisorProfile)
            .FirstAsync(x => x.Id == model.UserId);

        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        user.IsActive = model.IsActive;

        if (model.Role == ApplicationRoles.Student && user.StudentProfile is not null)
        {
            user.StudentProfile.StudentIdNumber = model.StudentIdNumber?.Trim() ?? user.StudentProfile.StudentIdNumber;
            user.StudentProfile.Department = model.Department.Trim();
            user.StudentProfile.ContactNumber = model.ContactNumber.Trim();
            user.StudentProfile.ProjectOwnershipType = model.ProjectOwnershipType;
        }
        else if (model.Role == ApplicationRoles.Supervisor && user.SupervisorProfile is not null)
        {
            user.SupervisorProfile.EmployeeId = model.EmployeeId?.Trim() ?? user.SupervisorProfile.EmployeeId;
            user.SupervisorProfile.Department = model.Department.Trim();
            user.SupervisorProfile.ContactNumber = model.ContactNumber.Trim();
            user.SupervisorProfile.Bio = model.Bio?.Trim();
        }

        await _context.SaveChangesAsync();
        await _userManager.UpdateAsync(user);
        await _auditService.LogAsync("UserUpdated", nameof(ApplicationUser), user.Id, user.Email, user.Id);
    }

    public Task<ApplicationUser?> GetUserAsync(string userId)
        => _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

    public Task<StudentProfile?> GetStudentProfileAsync(string userId)
        => _context.StudentProfiles.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);

    public Task<SupervisorProfile?> GetSupervisorProfileAsync(string userId)
        => _context.SupervisorProfiles
            .Include(x => x.User)
            .Include(x => x.ResearchAreas)
            .FirstOrDefaultAsync(x => x.UserId == userId);

    public async Task UpdateStudentProfileAsync(string userId, StudentProfileViewModel model)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        var profile = await _context.StudentProfiles.FirstAsync(x => x.UserId == userId);
        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        profile.StudentIdNumber = model.StudentIdNumber.Trim();
        profile.Department = model.Department.Trim();
        profile.ContactNumber = model.ContactNumber.Trim();
        profile.ProjectOwnershipType = model.ProjectOwnershipType;
        await _context.SaveChangesAsync();
        await _userManager.UpdateAsync(user);
        await _auditService.LogAsync("StudentProfileUpdated", nameof(StudentProfile), profile.Id.ToString(), model.FullName, userId);
    }

    public async Task UpdateSupervisorProfileAsync(string userId, SupervisorProfileViewModel model)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        var profile = await _context.SupervisorProfiles.FirstAsync(x => x.UserId == userId);
        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        profile.EmployeeId = model.EmployeeId.Trim();
        profile.Department = model.Department.Trim();
        profile.ContactNumber = model.ContactNumber.Trim();
        profile.Bio = model.Bio?.Trim();
        await _context.SaveChangesAsync();
        await _userManager.UpdateAsync(user);
        await UpdateSupervisorExpertiseAsync(userId, model.SelectedResearchAreaIds);
        await _auditService.LogAsync("SupervisorProfileUpdated", nameof(SupervisorProfile), profile.Id.ToString(), model.FullName, userId);
    }

    public async Task UpdateSupervisorExpertiseAsync(string userId, IEnumerable<int> researchAreaIds)
    {
        var supervisor = await _context.SupervisorProfiles
            .Include(x => x.ResearchAreas)
            .FirstAsync(x => x.UserId == userId);

        var desiredIds = researchAreaIds.Distinct().ToHashSet();
        var existingIds = supervisor.ResearchAreas.Select(x => x.ResearchAreaId).ToHashSet();
        var toRemove = supervisor.ResearchAreas.Where(x => !desiredIds.Contains(x.ResearchAreaId)).ToList();
        var toAdd = desiredIds.Where(x => !existingIds.Contains(x))
            .Select(x => new SupervisorResearchArea { SupervisorProfileId = supervisor.Id, ResearchAreaId = x })
            .ToList();

        _context.SupervisorResearchAreas.RemoveRange(toRemove);
        await _context.SupervisorResearchAreas.AddRangeAsync(toAdd);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("SupervisorExpertiseUpdated", nameof(SupervisorProfile), supervisor.Id.ToString(), string.Join(", ", desiredIds), userId);
    }

    private async Task CreateInternalAsync(RegisterViewModel model, string role)
    {
        var user = new ApplicationUser
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            UserName = model.Email.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            RoleDisplayName = role
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await _userManager.AddToRoleAsync(user, role);

        if (role == ApplicationRoles.Student)
        {
            await _context.StudentProfiles.AddAsync(new StudentProfile
            {
                UserId = user.Id,
                StudentIdNumber = model.StudentIdNumber?.Trim() ?? throw new InvalidOperationException("Student ID is required."),
                Department = model.Department.Trim(),
                ContactNumber = model.ContactNumber.Trim(),
                ProjectOwnershipType = model.ProjectOwnershipType
            });
        }
        else if (role == ApplicationRoles.Supervisor)
        {
            await _context.SupervisorProfiles.AddAsync(new SupervisorProfile
            {
                UserId = user.Id,
                EmployeeId = model.EmployeeId?.Trim() ?? throw new InvalidOperationException("Employee ID is required."),
                Department = model.Department.Trim(),
                ContactNumber = model.ContactNumber.Trim(),
                Bio = model.Bio?.Trim()
            });
        }

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("UserCreated", nameof(ApplicationUser), user.Id, $"{role}: {user.Email}");
    }
}
