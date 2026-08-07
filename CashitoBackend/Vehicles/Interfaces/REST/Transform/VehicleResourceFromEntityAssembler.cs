using CashitoBackend.Vehicles.Domain.Model.Aggregates;
using CashitoBackend.Vehicles.Interfaces.REST.Resources;

namespace CashitoBackend.Vehicles.Interfaces.REST.Transform;

public static class VehicleResourceFromEntityAssembler
{
    public static VehicleResource ToResourceFromEntity(Vehicle entity)
    {
        return new VehicleResource(
            entity.Id,
            entity.Brand,
            entity.Model,
            entity.Price,
            entity.Currency.ToString(),
            entity.Year,
            entity.Type.ToString()
        );
    }
}