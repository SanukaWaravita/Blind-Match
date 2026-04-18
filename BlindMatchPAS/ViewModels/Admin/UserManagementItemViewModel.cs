namespace BlindMatchPAS.ViewModels.Admin;

public class UserManagementItemViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Department { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
