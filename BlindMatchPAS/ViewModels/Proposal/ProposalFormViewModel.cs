using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.ViewModels.Proposal;

public class ProposalFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(160, MinimumLength = 10)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2500, MinimumLength = 80)]
    public string Abstract { get; set; } = string.Empty;

    [Required, StringLength(600, MinimumLength = 2)]
    public string TechnicalStack { get; set; } = string.Empty;

    [Required]
    public int ResearchAreaId { get; set; }

    [StringLength(2000)]
    public string? AdditionalDescription { get; set; }

    public ProposalStatus? Status { get; set; }

    public IEnumerable<SelectListItem> ResearchAreas { get; set; } = Enumerable.Empty<SelectListItem>();
}
