using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Interfaces.REST.Resources;

namespace CashitoBackend.IAM.Interfaces.REST.Transform;

public static class ChangePasswordCommandFromResourceAssembler
{
    public static ChangePasswordCommand ToCommandFromResource(
        int userId,
        ChangePasswordResource resource)
    {
        return new ChangePasswordCommand(
            userId,
            resource.CurrentPassword,
            resource.NewPassword
        );
    }
}