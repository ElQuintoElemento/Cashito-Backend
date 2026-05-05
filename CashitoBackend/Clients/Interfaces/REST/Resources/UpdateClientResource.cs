namespace CashitoBackend.Clients.Interfaces.REST.Resources;

public record UpdateClientResource(
    string FirstName,
    string LastName,
    decimal MonthlyIncome,
    string Phone,
    string Email
);