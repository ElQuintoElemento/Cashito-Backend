using CashitoBackend.Vehicles.Domain.Model.Commands;
using CashitoBackend.Vehicles.Interfaces.REST.Resources;

namespace CashitoBackend.Vehicles.Interfaces.REST.Transform;

public static class CreateVehicleCommandFromResourceAssembler
{
    public static CreateVehicleCommand ToCommandFromResource(CreateVehicleResource resource)
    {
        return new CreateVehicleCommand(
            resource.Brand,
            resource.Model,
            resource.Price,
            resource.Currency,
            resource.Year,
            resource.Type
        );
    }
}