using CashitoBackend.Credits.Domain.Model.Commands;
using CashitoBackend.Credits.Interfaces.REST.Resources;

namespace CashitoBackend.Credits.Interfaces.REST.Transform;

public static class CreateCreditCommandFromResourceAssembler
{
    public static CreateCreditCommand ToCommandFromResource(CreateCreditResource resource)
    {
        return new CreateCreditCommand(
            resource.ClientId,
            resource.VehicleId,
            resource.VehiclePrice,
            resource.DownPayment,
            resource.InterestRate,
            resource.TermMonths,
            resource.RateType,
            resource.GracePeriod,
            resource.Insurance
        );
    }
}