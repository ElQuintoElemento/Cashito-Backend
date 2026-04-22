using CashitoBackend.Vehicles.Domain.Model.Commands;
using CashitoBackend.Vehicles.Interfaces.REST.Resources;

namespace CashitoBackend.Vehicles.Interfaces.REST.Transform;

public static class UpdateVehicleCommandFromResourceAssembler
{
    public static UpdateVehicleCommand ToCommandFromResource(int id, UpdateVehicleResource resource)
    {
        return new UpdateVehicleCommand(
            id,
            resource.Brand,
            resource.Model,
            resource.Price,
            resource.Currency,
            resource.Year,
            resource.Type
        );
    }
}