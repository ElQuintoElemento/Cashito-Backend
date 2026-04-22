namespace CashitoBackend.Credits.Interfaces.REST.Resources;

public record CreateCreditResource(
    int ClientId,
    int VehicleId,
    decimal VehiclePrice,
    decimal DownPayment,
    decimal InterestRate,
    int TermMonths,
    string RateType,
    int GracePeriod,
    decimal Insurance
);