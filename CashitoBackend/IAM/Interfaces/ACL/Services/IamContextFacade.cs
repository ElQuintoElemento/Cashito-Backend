using CashitoBackend.IAM.Domain.Model.Queries;
using CashitoBackend.IAM.Domain.Services;

namespace CashitoBackend.IAM.Interfaces.ACL.Services;

public class IamContextFacade(
    IUserQueryService userQueryService
) : IIamContextFacade
{

    public async Task<int> FetchUserIdByUsername(string username)
    {
        var user = await userQueryService.Handle(
            new GetUserByUsernameQuery(username)
        );

        return user?.Id ?? 0;
    }

    public async Task<string> FetchUsernameByUserId(int userId)
    {
        var user = await userQueryService.Handle(
            new GetUserByIdQuery(userId)
        );

        return user?.Username ?? string.Empty;
    }
}