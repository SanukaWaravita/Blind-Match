using BlindMatchPAS.ViewModels.Admin;
using BlindMatchPAS.ViewModels.Student;
using BlindMatchPAS.ViewModels.Supervisor;

namespace BlindMatchPAS.Interfaces;

public interface IDashboardService
{
    Task<StudentDashboardViewModel> GetStudentDashboardAsync(string userId);
    Task<SupervisorDashboardViewModel> GetSupervisorDashboardAsync(string userId);
    Task<AdminDashboardViewModel> GetAdminDashboardAsync();
    Task<ReportsViewModel> GetReportsAsync();
}
