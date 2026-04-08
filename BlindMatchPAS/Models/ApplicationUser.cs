using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BlindMatchPAS.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(64)]
    public string? RoleDisplayName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StudentProfile? StudentProfile { get; set; }

    public SupervisorProfile? SupervisorProfile { get; set; }
}
