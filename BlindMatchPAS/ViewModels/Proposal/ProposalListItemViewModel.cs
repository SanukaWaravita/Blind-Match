using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.ViewModels.Proposal;

public class ProposalListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string TechnicalStack { get; set; } = string.Empty;
    public string ResearchArea { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsMatched { get; set; }
    public bool IsAnonymous { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public string? SupervisorName { get; set; }
}
