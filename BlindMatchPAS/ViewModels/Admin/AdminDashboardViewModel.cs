namespace BlindMatchPAS.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalSupervisors { get; set; }
    public int TotalProposals { get; set; }
    public int PendingProposals { get; set; }
    public int MatchedProposals { get; set; }
    public int WithdrawnProposals { get; set; }
    public List<RecentMatchViewModel> RecentMatches { get; set; } = [];
    public List<RecentActivityViewModel> RecentActivity { get; set; } = [];
}
