namespace BlindMatchPAS.ViewModels.Admin;

public class ReportsViewModel
{
    public List<StatusSummaryViewModel> ProposalStatusSummary { get; set; } = [];
    public List<AreaSummaryViewModel> ProposalsByArea { get; set; } = [];
    public List<DepartmentSummaryViewModel> SupervisorsByDepartment { get; set; } = [];
    public List<RecentMatchViewModel> RecentMatches { get; set; } = [];
}
