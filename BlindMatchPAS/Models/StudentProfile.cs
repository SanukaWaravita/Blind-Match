using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Models;

public class StudentProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [RegularExpression(@"^[A-Za-z0-9\-]{5,20}$", ErrorMessage = "Student ID must be 5-20 letters, numbers, or dashes.")]
    public string StudentIdNumber { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Department { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Contact number must be 10-15 digits.")]
    public string ContactNumber { get; set; } = string.Empty;

    public ProjectOwnershipType ProjectOwnershipType { get; set; } = ProjectOwnershipType.Individual;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    public ICollection<ProjectProposal> Proposals { get; set; } = new List<ProjectProposal>();
}
