using BlindMatchPAS.Models;

namespace BlindMatchPAS.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string entityId, string? details = null, string? actorUserId = null);
    Task<List<AuditLog>> GetRecentAsync(int count = 50);
}
