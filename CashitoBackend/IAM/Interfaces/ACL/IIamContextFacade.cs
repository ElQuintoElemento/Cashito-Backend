namespace CashitoBackend.IAM.Interfaces.ACL;

public interface IIamContextFacade
{
    Task<int> FetchUserIdByUsername(string username);
    Task<string> FetchUsernameByUserId(int userId);
}