namespace CashitoBackend.Clients.Interfaces.REST.Resources;

public record ClientResource(
    int Id,
    string Dni,
    string FirstName,
    string LastName,
    decimal MonthlyIncome,
    string IncomeCurrency,
    string Phone,
    string Email
);