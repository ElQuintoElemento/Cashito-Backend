using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Domain.Model.Queries;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using System.Security.Claims;

namespace CashitoBackend.IAM.Infrastructure.Pipeline.Middleware.Components;

/**
 * RequestAuthorizationMiddleware is a custom middleware.
 * This middleware is used to authorize requests.
 * It validates a token is included in the request header and that the token is valid.
 * If the token is valid then it sets the user in HttpContext.Items["User"].
 */
public class RequestAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestAuthorizationMiddleware> _logger;

    public RequestAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<RequestAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    /**
     * InvokeAsync is called by the ASP.NET Core runtime.
     * It is used to authorize requests.
     * It validates a token is included in the request header and that the token is valid.
     * If the token is valid then it sets the user in HttpContext.Items["User"].
     */
    public async Task InvokeAsync(
        HttpContext context,
        IUserQueryService userQueryService,
        ITokenService tokenService)
    {
        
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/_framework"))
        {
            await _next(context);
            return;
        }
        
        // skip authorization if endpoint is decorated with [AllowAnonymous] attribute
        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata
            .GetOrderedMetadata<AllowAnonymousAttribute>()
            .Any() == true;
        Console.WriteLine($"Allow Anonymous is {allowAnonymous}");
        
        if (allowAnonymous)
        {
            Console.WriteLine("Skipping authorization");
            // [AllowAnonymous] attribute is set, so skip authorization
            await _next(context);
            return;
        }
        Console.WriteLine("Entering authorization");
        // get token from request header
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?
            .Split(" ")
            .Last();
        

        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("El encabezado Authorization es obligatorio");
        }
        
        // validate token
        var claims = await tokenService.ValidateToken(token);

        // if token is invalid then throw exception
        if (claims == null)
            throw new UnauthorizedAccessException("Token inválido o expirado.");
        
        var userId = claims.Value;
        
        var user = await userQueryService.Handle(
            new GetUserByIdQuery(userId)
        );
        
        var identityClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        
        identityClaims.AddRange(
            user.Roles.Select(r =>
                new Claim(ClaimTypes.Role, r.Name.ToString())
            )
        );

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(identityClaims, "Custom")
        );

        _logger.LogInformation("User authenticated: {UserId}", user.Id);

        await _next(context);
        
    }
}