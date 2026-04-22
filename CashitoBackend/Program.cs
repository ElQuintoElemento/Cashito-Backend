using CashitoBackend.Clients.Application.Application.CommandServices;
using CashitoBackend.Clients.Application.Application.QueryServices;
using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Clients.Domain.Services;
using CashitoBackend.Clients.Infrastructure.Persistence.EFC.Repositories;
using CashitoBackend.IAM.Application.Internal.CommandServices;
using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Application.Internal.QueryServices;
using CashitoBackend.IAM.Domain.Repositories;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.IAM.Infrastructure.Hashing.BCrypt.Services;
using CashitoBackend.IAM.Infrastructure.Persistence.EFC.Repositories;
using CashitoBackend.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using CashitoBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using CashitoBackend.IAM.Infrastructure.Tokens.JWT.Services;
using CashitoBackend.IAM.Interfaces.ACL;
using CashitoBackend.IAM.Interfaces.ACL.Services;
using CashitoBackend.Shared.Domain.Events;
using CashitoBackend.Shared.Domain.Repositories;
using CashitoBackend.Shared.Infrastructure.DomainEvents;
using CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Configuration;
using CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Middleware;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ───────────── Controllers & routing ─────────────
builder.Services.AddRouting(o => o.LowercaseUrls = true);
builder.Services.AddControllers(o =>
{
    o.Conventions.Add(new KebabCaseRouteNamingConvention());
});

// ───────────── Autenticación "passthrough" ─────────────
builder.Services.AddAuthentication("Custom")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
            CashitoBackend.IAM.Infrastructure.Authorization.PassthroughAuthenticationHandler>
        ("Custom", _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("RoleAdmin"));
});

// ───────────── CORS ─────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        p => p.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ───────────── DbContext (MySQL) ─────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    else
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error);
});


// ───────────── Swagger / OpenAPI ─────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CashitoBackend API",
        Version = "v1",
        Description = "Cashito Backend",
        TermsOfService = new Uri("https://awawatech.com/tos"),
        Contact = new OpenApiContact
        {
            Name = "El Quinto Elemento",
            Email = "andrestheb@gmail.com"
        },
        License = new OpenApiLicense
        {
            Name = "Apache 2.0",
            Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
        }
    });
    // JWT Bearer config
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    options.EnableAnnotations();
});


// Dependency Injection

// Clients Bounded Context

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientCommandService, ClientCommandService>();
builder.Services.AddScoped<IClientQueryService, ClientQueryService>();

// Shared Bounded Context
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// IAM Bounded Context Injection Configuration

// TokenSettings Configuration

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

// ───────────── Build & DB ensure ─────────────a
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// ───────────── HTTP pipeline ─────────────
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAllPolicy");

// Habilita el enrutamiento para que el endpoint esté disponible en middlewares anteriores
app.UseRouting();

app.UseHttpsRedirection();

app.UseRequestAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
