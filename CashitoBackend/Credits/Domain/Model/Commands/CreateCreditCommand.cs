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
    decimal Insurance,
    decimal OpportunityRate,
    string? Capitalization = null,
    decimal DesgravamenInsuranceRate = 0,
    decimal VehicularInsuranceRate = 0,
    decimal Portes = 0,
    decimal DisbursementFee = 0,
    decimal EvaluationFee = 0,
    decimal NotaryExpenses = 0,
    decimal SoatAmount = 0,
    decimal OtherExpenses = 0,
    decimal BalloonPaymentPercentage = 0
);