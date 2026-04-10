using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels.Admin;

namespace BlindMatchPAS.Interfaces;

public interface IResearchAreaService
{
    Task<List<ResearchArea>> GetAllAsync(bool includeInactive = false);
    Task<ResearchArea?> GetByIdAsync(int id);
    Task<int> CreateAsync(ResearchAreaFormViewModel model);
    Task UpdateAsync(ResearchAreaFormViewModel model);
    Task ToggleActiveAsync(int id);
}
