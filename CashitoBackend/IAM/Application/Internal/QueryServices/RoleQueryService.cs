using CashitoBackend.IAM.Domain.Model.Entities;
using CashitoBackend.IAM.Domain.Model.Queries;
using CashitoBackend.IAM.Domain.Repositories;
using CashitoBackend.IAM.Domain.Services;

namespace CashitoBackend.IAM.Application.Internal.QueryServices;

public class RoleQueryService(IRoleRepository roleRepository) : IRoleQueryService
{
    public async Task<IEnumerable<Role>> Handle(GetAllRolesQuery query)
    {
        return await roleRepository.ListAsync();
    }

    public async Task<Role?> Handle(GetRoleByNameQuery query)
    {
        return await roleRepository.FindByNameAsync(query.Name);
    }
} 