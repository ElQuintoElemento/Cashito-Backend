namespace CashitoBackend.Clients.Domain.Model.Commands;

public record CreateClientCommand(
    string Dni,
    string FirstName,
    string LastName,
    decimal MonthlyIncome,
    string Phone
);