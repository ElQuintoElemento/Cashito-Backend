namespace CashitoBackend.Credits.Interfaces.REST.Resources;

public record CreditResource(
    int Id,
    int ClientId,
    int VehicleId,

    decimal VehiclePrice,
    string Currency,
    decimal DownPayment,
    decimal FinancedAmount,

    decimal InterestRate,
    int TermMonths,
    string RateType,

    int GracePeriod,
    string GraceType,

    decimal Insurance,

    decimal Tcea,
    decimal Van,
    decimal Tir,

    string Status,
    string PublicToken,
    string? Capitalization,
    decimal DesgravamenInsuranceRate,
    decimal VehicularInsuranceRate,
    decimal Portes,
    decimal DisbursementFee,
    decimal EvaluationFee,
    decimal NotaryExpenses,
    decimal SoatAmount,
    decimal OtherExpenses,
    decimal AmortizableCapital,
    decimal InitialPaymentPercentage,
    decimal BalloonPaymentPercentage,
    decimal BalloonPaymentAmount,
    decimal BaseInstallment,
    decimal OpportunityRate
);