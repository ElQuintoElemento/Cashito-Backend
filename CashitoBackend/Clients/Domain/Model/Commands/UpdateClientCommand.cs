namespace CashitoBackend.Clients.Domain.Model.Commands;

public record UpdateClientCommand(
    int Id,
    string FirstName,
    string LastName,
    decimal MonthlyIncome,
    string Phone
);