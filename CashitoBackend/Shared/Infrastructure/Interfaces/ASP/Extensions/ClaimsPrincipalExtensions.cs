using System.Security.Claims;

namespace CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Extensions
{
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            throw new UnauthorizedAccessException("User ID not found in token");

        return int.Parse(claim.Value);
    }
}
}