using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.ViewModels.Account;

public class RegisterViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [StringLength(20)]
    public string? StudentIdNumber { get; set; }

    [StringLength(20)]
    public string? EmployeeId { get; set; }

    [Required, StringLength(120)]
    public string Department { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [RegularExpression(@"^\+?[0-9]{10,15}$")]
    public string ContactNumber { get; set; } = string.Empty;

    public ProjectOwnershipType ProjectOwnershipType { get; set; } = ProjectOwnershipType.Individual;

    [StringLength(800)]
    public string? Bio { get; set; }
}
