namespace CashitoBackend.Clients.Interfaces.REST.Resources;

public record UpdateClientResource(
    string FirstName,
    string LastName,
    decimal MonthlyIncome,
    string IncomeCurrency,
    string Phone,
    string Email
);