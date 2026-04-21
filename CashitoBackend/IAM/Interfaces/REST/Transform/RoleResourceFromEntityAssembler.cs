using CashitoBackend.IAM.Domain.Model.Entities;
using CashitoBackend.IAM.Interfaces.REST.Resources;

namespace CashitoBackend.IAM.Interfaces.REST.Transform;

public static class RoleResourceFromEntityAssembler
{
    public static RoleResource ToResourceFromEntity(Role role)
        => new(role.Id, role.Name.ToString());
} 