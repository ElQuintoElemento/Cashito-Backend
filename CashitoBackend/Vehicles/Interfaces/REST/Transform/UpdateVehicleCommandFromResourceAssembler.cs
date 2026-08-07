using CashitoBackend.Shared.Domain.Model.ValueObjects;
using CashitoBackend.Vehicles.Domain.Model.Commands;
using CashitoBackend.Vehicles.Domain.Model.ValueObjects;
using CashitoBackend.Vehicles.Interfaces.REST.Resources;

namespace CashitoBackend.Vehicles.Interfaces.REST.Transform;

public static class UpdateVehicleCommandFromResourceAssembler
{
    public static UpdateVehicleCommand ToCommandFromResource(int id, UpdateVehicleResource resource)
    {
        if (!Enum.TryParse<Currency>(resource.Currency, true, out var currency))
            throw new Exception("Invalid currency");

        if (!Enum.TryParse<VehicleType>(resource.Type, true, out var type))
            throw new Exception("Invalid vehicle type");

        return new UpdateVehicleCommand(
            id,
            resource.Brand,
            resource.Model,
            resource.Price,
            currency,
            resource.Year,
            type
        );
    }
}