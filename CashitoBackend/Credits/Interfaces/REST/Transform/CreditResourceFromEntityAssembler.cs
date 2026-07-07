using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Interfaces.REST.Resources;

namespace CashitoBackend.Credits.Interfaces.REST.Transform;

public static class CreditResourceFromEntityAssembler
{
    public static CreditResource ToResourceFromEntity(Credit entity)
    {
        return new CreditResource(
            entity.Id,
            entity.ClientId,
            entity.VehicleId,

            entity.VehiclePrice,
            entity.Currency.ToString(),
            entity.DownPayment,
            entity.FinancedAmount,

            entity.InterestRate,
            entity.TermMonths,
            entity.RateType,

            entity.GracePeriod,
            entity.GraceType.ToString(),

            entity.Insurance,

            entity.Tcea,
            entity.Van,
            entity.Tir,

            entity.Status.ToString(),
            entity.PublicToken,
            entity.Capitalization,
            entity.DesgravamenInsuranceRate,
            entity.VehicularInsuranceRate,
            entity.Portes,
            entity.DisbursementFee,
            entity.EvaluationFee,
            entity.NotaryExpenses,
            entity.SoatAmount,
            entity.OtherExpenses,
            entity.AmortizableCapital,
            entity.InitialPaymentPercentage,
            entity.BalloonPaymentPercentage,
            entity.BalloonPaymentAmount,
            entity.BaseInstallment,
            entity.OpportunityRate
        );
    }
}