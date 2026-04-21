namespace CashitoBackend.IAM.Domain.Model.Entities;

public class TokenData
{
    public int UserId { get; set; }
    public List<string> Roles { get; set; } = new();
}