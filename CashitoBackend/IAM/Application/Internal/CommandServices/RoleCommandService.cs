using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Domain.Model.Entities;
using CashitoBackend.IAM.Domain.Model.ValueObjects;
using CashitoBackend.IAM.Domain.Repositories;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.Shared.Domain.Repositories;

namespace CashitoBackend.IAM.Application.Internal.CommandServices;

public class RoleCommandService(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRoleCommandService
{
    public async Task Handle(SeedRolesCommand command)
    {
        // Default roles from enum
        var defaults = Enum.GetValues<Roles>();
        foreach (var roleName in defaults)
        {
            var nameStr = roleName.ToString();
            if (await roleRepository.FindByNameAsync(nameStr) == null)
            {
                await roleRepository.AddAsync(new Role(roleName));
            }
        }
        await unitOfWork.CompleteAsync();
    }
} 