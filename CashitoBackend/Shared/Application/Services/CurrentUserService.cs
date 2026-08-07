using CashitoBackend.Shared.Domain.Services;

namespace CashitoBackend.Shared.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetUserId()
    {
        var userId = _httpContextAccessor
            .HttpContext?
            .User?
            .FindFirst("sub")?.Value;

        if (userId == null)
            throw new Exception("Usuario no autenticado");

        return int.Parse(userId);
    }
}