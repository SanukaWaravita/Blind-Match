using BlindMatchPAS.ViewModels.Proposal;

namespace BlindMatchPAS.ViewModels.Student;

public class StudentDashboardViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public int ProposalCount { get; set; }
    public int PendingCount { get; set; }
    public int MatchedCount { get; set; }
    public List<ProposalListItemViewModel> Proposals { get; set; } = [];
}
