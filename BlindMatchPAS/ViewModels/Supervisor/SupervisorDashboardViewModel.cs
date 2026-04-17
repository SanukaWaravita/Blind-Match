using BlindMatchPAS.ViewModels.Proposal;

namespace BlindMatchPAS.ViewModels.Supervisor;

public class SupervisorDashboardViewModel
{
    public string SupervisorName { get; set; } = string.Empty;
    public int AnonymousProposalCount { get; set; }
    public int ActiveInterestCount { get; set; }
    public int MatchCount { get; set; }
    public List<ProposalListItemViewModel> AnonymousProposals { get; set; } = [];
}
