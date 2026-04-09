using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Action { get; set; } = string.Empty;

    [StringLength(450)]
    public string? ActorUserId { get; set; }

    [Required, StringLength(120)]
    public string EntityName { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string EntityId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(2000)]
    public string? Details { get; set; }
}
