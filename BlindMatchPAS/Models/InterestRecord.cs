using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Models;

public class InterestRecord
{
    public int Id { get; set; }
    public int ProposalId { get; set; }
    public int SupervisorProfileId { get; set; }
    public DateTime ExpressedAt { get; set; } = DateTime.UtcNow;
    public InterestStatus Status { get; set; } = InterestStatus.Active;

    public ProjectProposal Proposal { get; set; } = null!;
    public SupervisorProfile SupervisorProfile { get; set; } = null!;
}
