using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.IAM.Interfaces.REST.Resources;

namespace CashitoBackend.IAM.Interfaces.REST.Transform;

public static class AuthenticatedUserResourceFromEntityAssembler
{
    public static AuthenticatedUserResource ToResourceFromEntity(
        User user, string token)
    {
        return new AuthenticatedUserResource(user.Id, user.Username, token);
    }
}