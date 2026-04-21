using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Interfaces.REST.Resources;

namespace CashitoBackend.IAM.Interfaces.REST.Transform;

public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        return new SignInCommand(resource.Username, resource.Password);
    }
}