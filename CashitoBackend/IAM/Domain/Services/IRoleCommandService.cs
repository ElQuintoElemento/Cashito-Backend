using CashitoBackend.IAM.Domain.Model.Commands;

namespace CashitoBackend.IAM.Domain.Services;

public interface IRoleCommandService
{
    Task Handle(SeedRolesCommand command);
} 