using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Interfaces.REST.Resources;
using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.IAM.Interfaces.REST.Transform;

public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        return new SignUpCommand(
            resource.Username,
            resource.Password,
            resource.Email,
            resource.FirstName,
            resource.LastName
        );
    }
}