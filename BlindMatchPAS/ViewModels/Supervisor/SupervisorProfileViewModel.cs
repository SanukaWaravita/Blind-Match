using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.ViewModels.Supervisor;

public class SupervisorProfileViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [RegularExpression(@"^[A-Za-z0-9\-]{4,20}$")]
    public string EmployeeId { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Department { get; set; } = string.Empty;

    [StringLength(800)]
    public string? Bio { get; set; }

    [Required, StringLength(20)]
    [RegularExpression(@"^\+?[0-9]{10,15}$")]
    public string ContactNumber { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public List<int> SelectedResearchAreaIds { get; set; } = [];
}
