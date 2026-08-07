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
    string Status,
    decimal BaseInstallment,
    decimal BeginningBalance,
    decimal DesgravamenInsurance,
    decimal VehicularInsurance,
    decimal Portes,
    decimal OtherExpenses,
    decimal CashFlow,
    bool IsBalloon,
    decimal BalloonAmount
);