namespace CashitoBackend.Credits.Interfaces.REST.Resources;

public record SimulateCreditResource(
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