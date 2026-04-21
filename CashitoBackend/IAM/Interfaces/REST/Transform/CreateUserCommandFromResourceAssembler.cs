using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Interfaces.REST.Resources;

namespace CashitoBackend.IAM.Interfaces.REST.Transform;

public static class CreateUserCommandFromResourceAssembler
{
    public static CreateUserCommand ToCommandFromResource(CreateUserResource resource)
    {
        return new CreateUserCommand(
            resource.Username,
            resource.Password,
            resource.Email,
            resource.FirstName,
            resource.LastName,
            resource.Roles);
    }
} 