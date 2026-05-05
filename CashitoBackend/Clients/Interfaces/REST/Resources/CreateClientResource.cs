namespace CashitoBackend.Clients.Interfaces.REST.Resources;

public record CreateClientResource(
    string Dni,
    string FirstName,
    string LastName,
    decimal MonthlyIncome,
    string Phone,
    string Email
);