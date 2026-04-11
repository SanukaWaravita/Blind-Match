using BlindMatchPAS.Data;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public class ResearchAreaService : IResearchAreaService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public ResearchAreaService(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public Task<List<ResearchArea>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.ResearchAreas.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return query.OrderBy(x => x.Name).ToListAsync();
    }

    public Task<ResearchArea?> GetByIdAsync(int id)
        => _context.ResearchAreas.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<int> CreateAsync(ResearchAreaFormViewModel model)
    {
        var entity = new ResearchArea
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            IsActive = model.IsActive
        };

        await _context.ResearchAreas.AddAsync(entity);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("ResearchAreaCreated", nameof(ResearchArea), entity.Id.ToString(), entity.Name);
        return entity.Id;
    }

    public async Task UpdateAsync(ResearchAreaFormViewModel model)
    {
        var entity = await _context.ResearchAreas.FirstAsync(x => x.Id == model.Id);
        entity.Name = model.Name.Trim();
        entity.Description = model.Description?.Trim();
        entity.IsActive = model.IsActive;
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("ResearchAreaUpdated", nameof(ResearchArea), entity.Id.ToString(), entity.Name);
    }

    public async Task ToggleActiveAsync(int id)
    {
        var entity = await _context.ResearchAreas.FirstAsync(x => x.Id == id);
        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("ResearchAreaToggled", nameof(ResearchArea), entity.Id.ToString(), $"Active: {entity.IsActive}");
    }
}
