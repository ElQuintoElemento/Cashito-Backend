namespace CashitoBackend.Credits.Interfaces.REST.Resources;

public record CreditResource(
    int Id,
    int ClientId,
    int VehicleId,

    decimal VehiclePrice,
    decimal DownPayment,
    decimal FinancedAmount,

    decimal InterestRate,
    int TermMonths,
    string RateType,
    int GracePeriod,
    decimal Insurance,

    decimal Tcea,
    decimal Van,
    decimal Tir,

    string Status,
    string PublicToken
);