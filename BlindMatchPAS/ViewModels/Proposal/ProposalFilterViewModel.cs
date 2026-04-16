using BlindMatchPAS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BlindMatchPAS.ViewModels.Proposal;

public class ProposalFilterViewModel
{
    public string? Keyword { get; set; }
    public int? ResearchAreaId { get; set; }
    public ProposalStatus? Status { get; set; }
    public string? TechnicalStack { get; set; }
    public IEnumerable<SelectListItem> ResearchAreas { get; set; } = Enumerable.Empty<SelectListItem>();
}
