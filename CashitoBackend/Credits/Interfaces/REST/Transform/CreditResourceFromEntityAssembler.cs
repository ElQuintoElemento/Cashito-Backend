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
            entity.PublicToken
        );
    }
}