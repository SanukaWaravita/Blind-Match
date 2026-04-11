using BlindMatchPAS.Data;
using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Models;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string action, string entityName, string entityId, string? details = null, string? actorUserId = null)
    {
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            ActorUserId = actorUserId
        });
        await _context.SaveChangesAsync();
    }

    public Task<List<AuditLog>> GetRecentAsync(int count = 50)
        => _context.AuditLogs.OrderByDescending(x => x.Timestamp).Take(count).ToListAsync();
}
