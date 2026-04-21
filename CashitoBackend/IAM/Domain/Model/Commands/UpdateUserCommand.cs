namespace CashitoBackend.IAM.Domain.Model.Commands;

public record UpdateUserCommand(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles
);