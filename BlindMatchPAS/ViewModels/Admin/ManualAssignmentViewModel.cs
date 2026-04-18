using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.ViewModels.Admin;

public class ManualAssignmentViewModel
{
    [Required]
    public int ProposalId { get; set; }

    [Required]
    public int SupervisorProfileId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public string ProposalTitle { get; set; } = string.Empty;
    public IEnumerable<SelectListItem> Supervisors { get; set; } = Enumerable.Empty<SelectListItem>();
}
