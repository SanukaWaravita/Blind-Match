using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

public class MatchRecord
{
    public int Id { get; set; }
    public int ProposalId { get; set; }
    public int SupervisorProfileId { get; set; }
    public bool ConfirmedBySupervisor { get; set; }
    public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? RevealedAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public ProjectProposal Proposal { get; set; } = null!;
    public SupervisorProfile SupervisorProfile { get; set; } = null!;
}
