using System.Security.Claims;

namespace CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Extensions
{
public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? long.Parse(claim.Value) : 0;
    }
    
}
}