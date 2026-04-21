using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Domain.Services;
using Microsoft.Extensions.Hosting;

namespace CashitoBackend.IAM.Application.Internal.EventHandlers;

public class SeedRolesHostedService(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var commandService = scope.ServiceProvider.GetRequiredService<IRoleCommandService>();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
} 