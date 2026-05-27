using CashitoBackend.Credits.Domain.Model.ValueObjects;
using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.Credits.Domain.Model.Commands;

public record CreateCreditCommand(
    int ClientId,
    int VehicleId,
    decimal VehiclePrice,
    Currency Currency,
    decimal DownPayment,
    decimal InterestRate,
    int TermMonths,
    string RateType,
    int GracePeriod,
    GraceType GraceType,
    decimal Insurance
);