namespace CashitoBackend.Credits.Interfaces.REST.Resources;

public record InstallmentResource(
    int Number,
    DateTime Date,
    decimal TotalPayment,
    decimal Interest,
    decimal Amortization,
    decimal RemainingBalance,
    bool IsPaid,
    DateTime? PaidAt,
    string Status
);