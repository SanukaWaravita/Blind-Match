using System.Security.Claims;

namespace BlindMatchPAS.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.NameIdentifier);
}
