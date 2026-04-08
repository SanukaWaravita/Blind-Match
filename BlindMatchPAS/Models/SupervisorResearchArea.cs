namespace BlindMatchPAS.Models;

public class SupervisorResearchArea
{
    public int SupervisorProfileId { get; set; }
    public int ResearchAreaId { get; set; }

    public SupervisorProfile SupervisorProfile { get; set; } = null!;
    public ResearchArea ResearchArea { get; set; } = null!;
}
