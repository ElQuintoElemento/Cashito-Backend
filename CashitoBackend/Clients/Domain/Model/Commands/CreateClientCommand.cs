using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.Clients.Domain.Model.Commands;

public record CreateClientCommand(
    string Dni,
    string FirstName,
    string LastName,
    decimal MonthlyIncome,
    Currency IncomeCurrency,
    string Phone,
    string Email
);