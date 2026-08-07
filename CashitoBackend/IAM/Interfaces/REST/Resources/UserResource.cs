namespace CashitoBackend.IAM.Interfaces.REST.Resources;

public record UserResource(
    int Id,
    string Username,
    string? FullName,
    string? Email
);