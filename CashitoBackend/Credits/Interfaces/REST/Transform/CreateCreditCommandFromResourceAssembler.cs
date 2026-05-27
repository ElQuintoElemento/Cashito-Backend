using CashitoBackend.Credits.Domain.Model.Commands;
using CashitoBackend.Credits.Domain.Model.ValueObjects;
using CashitoBackend.Credits.Interfaces.REST.Resources;
using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.Credits.Interfaces.REST.Transform;

public static class CreateCreditCommandFromResourceAssembler
{
    public static CreateCreditCommand ToCommandFromResource(CreateCreditResource resource)
    {
        if (!Enum.TryParse<Currency>(resource.Currency, true, out var currency))
            throw new Exception("Invalid currency");

        if (!Enum.TryParse<GraceType>(resource.GraceType, true, out var graceType))
            throw new Exception("Invalid grace type");

        return new CreateCreditCommand(
            resource.ClientId,
            resource.VehicleId,
            resource.VehiclePrice,
            currency,
            resource.DownPayment,
            resource.InterestRate,
            resource.TermMonths,
            resource.RateType,
            resource.GracePeriod,
            graceType,
            resource.Insurance
        );
    }
}