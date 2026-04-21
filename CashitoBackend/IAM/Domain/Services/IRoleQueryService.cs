using CashitoBackend.IAM.Domain.Model.Entities;
using CashitoBackend.IAM.Domain.Model.Queries;

namespace CashitoBackend.IAM.Domain.Services;

public interface IRoleQueryService
{
    Task<IEnumerable<Role>> Handle(GetAllRolesQuery query);
    Task<Role?> Handle(GetRoleByNameQuery query);
} 