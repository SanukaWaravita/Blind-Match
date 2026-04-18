namespace BlindMatchPAS.ViewModels.Admin;

public class AdminMatchViewModel
{
    public int ProposalId { get; set; }
    public string ProposalTitle { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public DateTime MatchedAt { get; set; }
    public bool IsActive { get; set; }
}
