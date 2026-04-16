using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.ViewModels.Student;

public class StudentProfileViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [RegularExpression(@"^[A-Za-z0-9\-]{5,20}$")]
    public string StudentIdNumber { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Department { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [RegularExpression(@"^\+?[0-9]{10,15}$")]
    public string ContactNumber { get; set; } = string.Empty;

    public ProjectOwnershipType ProjectOwnershipType { get; set; }

    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
