namespace CashitoBackend.Credits.Domain.Model.Commands;

public record CreateCreditCommand(
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