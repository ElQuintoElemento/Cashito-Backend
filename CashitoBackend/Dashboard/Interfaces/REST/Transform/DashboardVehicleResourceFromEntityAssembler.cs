using CashitoBackend.Dashboard.Interfaces.REST.Resources;
using CashitoBackend.Vehicles.Domain.Model.Aggregates;

namespace CashitoBackend.Dashboard.Interfaces.REST.Transform;

public static class DashboardVehicleResourceFromEntityAssembler
{
    public static DashboardVehicleResource ToResourceFromEntity(Vehicle entity) =>
        new DashboardVehicleResource(
            entity.Id,
            entity.Brand,
            entity.Model,
            entity.Year,
            entity.Price,
            entity.Currency.ToString());
}
