namespace CashitoBackend.IAM.Domain.Model.Commands;

public record SignUpCommand(
    string Username,
    string Password,
    string Email,
    string FirstName,
    string LastName
);