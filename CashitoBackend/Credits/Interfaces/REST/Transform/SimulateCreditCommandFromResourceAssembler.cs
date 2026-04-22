using CashitoBackend.Credits.Domain.Model.Commands;
using CashitoBackend.Credits.Interfaces.REST.Resources;

namespace CashitoBackend.Credits.Interfaces.REST.Transform;

public static class SimulateCreditCommandFromResourceAssembler
{
    public static SimulateCreditCommand ToCommandFromResource(SimulateCreditResource resource)
    {
        return new SimulateCreditCommand(
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