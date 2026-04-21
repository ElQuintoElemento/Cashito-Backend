using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Interfaces.REST.Resources;

namespace CashitoBackend.IAM.Interfaces.REST.Transform;

public static class UpdateUserCommandFromResourceAssembler
{
    public static UpdateUserCommand ToCommandFromResource(
        int id,
        UpdateUserResource resource)
    {
        return new UpdateUserCommand(
            id,
            resource.Email,
            resource.FirstName,
            resource.LastName,
            resource.Roles
        );
    }
}