using CashitoBackend.Vehicles.Domain.Model.Aggregates;
using CashitoBackend.Vehicles.Domain.Model.Queries;

namespace CashitoBackend.Vehicles.Domain.Services;

public interface IVehicleQueryService
{
    Task<IEnumerable<Vehicle>> Handle(GetAllVehiclesQuery query);

    Task<Vehicle?> Handle(GetVehicleByIdQuery query);
}