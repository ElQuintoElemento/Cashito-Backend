using System.IdentityModel.Tokens.Jwt;
using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Domain.Services;
using Microsoft.AspNetCore.Authorization;
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
            .GetMetadata<AllowAnonymousAttribute>() != null;
        
        if (allowAnonymous)
        {
            // [AllowAnonymous] attribute is set, so skip authorization
            await _next(context);
            return;
        }
        // get token from request header
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?
            .Split(" ")
            .Last();
        

        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("Authorization header is required");
        }
        
        // validate token
        var jwt = await tokenService.ValidateToken(token);

// if token is invalid then throw exception
        if (jwt == null)
            throw new UnauthorizedAccessException("Token inválido o expirado.");

        var userId = jwt.Claims
            .First(x => x.Type == JwtRegisteredClaimNames.Sub)
            .Value;
        
        var username = jwt.Claims
            .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.UniqueName)
            ?.Value;

        var identityClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim("sub", userId),
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        
        if (!string.IsNullOrWhiteSpace(username))
        {
            identityClaims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, username));
            identityClaims.Add(new Claim("unique_name", username));
            identityClaims.Add(new Claim(ClaimTypes.Name, username));
        }

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(identityClaims, "jwt")
        );

        _logger.LogInformation("User authenticated: {UserId}", userId);

        await _next(context);
    }
}