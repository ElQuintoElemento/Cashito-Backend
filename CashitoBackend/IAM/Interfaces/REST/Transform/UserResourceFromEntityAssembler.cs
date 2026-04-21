using System.Linq;
using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.IAM.Interfaces.REST.Resources;

namespace CashitoBackend.IAM.Interfaces.REST.Transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User user)
    {
        var fullName = string.Join(" ", new[]{user.FirstName, user.LastName}.Where(n=>!string.IsNullOrWhiteSpace(n)));
        var roles = user.Roles.Select(r => r.GetStringName());
        return new UserResource(
            user.Id,
            user.Username,
            string.IsNullOrWhiteSpace(fullName) ? null : fullName,
            user.Email?.ToString(),
            roles);
    }
}