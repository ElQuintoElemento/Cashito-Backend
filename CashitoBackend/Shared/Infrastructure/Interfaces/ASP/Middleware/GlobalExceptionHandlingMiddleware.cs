using System.Net;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using CashitoBackend.Shared.Domain.Exceptions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // excluye
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/_framework"))
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Determinar el código HTTP según el tipo de excepción
        var status = exception switch
        {
            //  Reglas de negocio
            NotFoundException             => HttpStatusCode.NotFound,
            BadRequestException           => HttpStatusCode.BadRequest,
            BusinessRuleException         => HttpStatusCode.UnprocessableEntity, // 422
            
            ValidationException           => HttpStatusCode.BadRequest,  // 400
            KeyNotFoundException          => HttpStatusCode.NotFound,    // 404
            UnauthorizedAccessException   => HttpStatusCode.Unauthorized,// 401
            SecurityTokenException        => HttpStatusCode.Unauthorized,// 401
            InvalidOperationException     => HttpStatusCode.Conflict,    // 409
            DbUpdateException             => HttpStatusCode.Conflict,    // 409 (unique constraint, etc.)
            _                             => HttpStatusCode.InternalServerError // 500
        };

        var message = status == HttpStatusCode.InternalServerError
            ? "Internal server error"
            : exception.Message;
        
        
        var responseObject = new
        {
            timestamp = DateTime.UtcNow,
            status    = (int)status,
            error     = status.ToString(),
            message,
            path      = context.Request.Path.ToString()
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)status;
        return context.Response.WriteAsync(JsonSerializer.Serialize(responseObject));
    }
}