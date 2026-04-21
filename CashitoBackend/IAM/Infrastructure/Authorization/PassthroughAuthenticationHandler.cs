using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CashitoBackend.IAM.Infrastructure.Authorization;

/// <summary>
/// Handler de autenticación "passthrough": no valida credenciales sino que
/// reutiliza el <see cref="HttpContext.User"/> que ya construyó el middleware
/// <c>RequestAuthorizationMiddleware</c>. Sirve para que el pipeline de
/// autorización estándar de ASP.NET Core (políticas, [Authorize]) disponga de
/// un esquema y pueda devolver 401/403 correctamente.
/// </summary>
public class PassthroughAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public PassthroughAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Si ya existe un usuario autenticado en el contexto, lo aceptamos.
        if (Context.User?.Identity is { IsAuthenticated: true })
        {
            var ticket = new AuthenticationTicket(Context.User, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // De lo contrario, indicamos que no se pudo autenticar (permitirá
        // que el atributo [AllowAnonymous] funcione, o que Authorization
        // devuelva 401/403 según corresponda).
        return Task.FromResult(AuthenticateResult.NoResult());
    }
} 