namespace CashitoBackend.IAM.Domain.Model.Commands;

public record CreateUserCommand(
    string Username,
    string Password,
    string Email,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles
);