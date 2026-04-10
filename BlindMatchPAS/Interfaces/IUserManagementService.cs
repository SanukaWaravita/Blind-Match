using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels.Account;
using BlindMatchPAS.ViewModels.Admin;
using BlindMatchPAS.ViewModels.Student;
using BlindMatchPAS.ViewModels.Supervisor;

namespace BlindMatchPAS.Interfaces;

public interface IUserManagementService
{
    Task RegisterStudentAsync(RegisterViewModel model);
    Task RegisterSupervisorAsync(RegisterViewModel model);
    Task<List<UserManagementItemViewModel>> GetUsersAsync(string? keyword, string? role);
    Task ToggleActiveAsync(string userId);
    Task CreateUserAsync(AdminCreateUserViewModel model);
    Task<AdminEditUserViewModel?> GetEditUserAsync(string userId);
    Task UpdateUserAsync(AdminEditUserViewModel model);
    Task<ApplicationUser?> GetUserAsync(string userId);
    Task<StudentProfile?> GetStudentProfileAsync(string userId);
    Task<SupervisorProfile?> GetSupervisorProfileAsync(string userId);
    Task UpdateStudentProfileAsync(string userId, StudentProfileViewModel model);
    Task UpdateSupervisorProfileAsync(string userId, SupervisorProfileViewModel model);
    Task UpdateSupervisorExpertiseAsync(string userId, IEnumerable<int> researchAreaIds);
}
