using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlindMatchPAS.Models;

public class SupervisorProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [RegularExpression(@"^[A-Za-z0-9\-]{4,20}$", ErrorMessage = "Employee ID must be 4-20 letters, numbers, or dashes.")]
    public string EmployeeId { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Department { get; set; } = string.Empty;

    [StringLength(800)]
    public string? Bio { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Contact number must be 10-15 digits.")]
    public string ContactNumber { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    public ICollection<SupervisorResearchArea> ResearchAreas { get; set; } = new List<SupervisorResearchArea>();
    public ICollection<MatchRecord> MatchRecords { get; set; } = new List<MatchRecord>();
    public ICollection<InterestRecord> InterestRecords { get; set; } = new List<InterestRecord>();
}
