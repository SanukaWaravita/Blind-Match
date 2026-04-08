using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

public class ResearchArea
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProjectProposal> Proposals { get; set; } = new List<ProjectProposal>();
    public ICollection<SupervisorResearchArea> SupervisorResearchAreas { get; set; } = new List<SupervisorResearchArea>();
}
