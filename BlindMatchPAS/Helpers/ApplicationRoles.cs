namespace BlindMatchPAS.Helpers;

public static class ApplicationRoles
{
    public const string Student = "Student";
    public const string Supervisor = "Supervisor";
    public const string Admin = "Admin";

    public static readonly string[] All = [Student, Supervisor, Admin];
}
