using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Models;

public class ProjectProposal
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2500, MinimumLength = 80)]
    public string Abstract { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string TechnicalStack { get; set; } = string.Empty;

    public int ResearchAreaId { get; set; }
    public int StudentProfileId { get; set; }

    [StringLength(2000)]
    public string? AdditionalDescription { get; set; }

    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public bool IsWithdrawn { get; set; }
    public bool IsMatched { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? MatchedAt { get; set; }
    public DateTime? WithdrawnAt { get; set; }

    public ResearchArea ResearchArea { get; set; } = null!;
    public StudentProfile StudentProfile { get; set; } = null!;
    public ICollection<MatchRecord> MatchRecords { get; set; } = new List<MatchRecord>();
    public ICollection<InterestRecord> InterestRecords { get; set; } = new List<InterestRecord>();
}
