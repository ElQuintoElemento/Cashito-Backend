namespace CashitoBackend.IAM.Interfaces.REST.Resources;

public record UpdateUserResource(
    string Email,
    string FirstName,
    string LastName
);