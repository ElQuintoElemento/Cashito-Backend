using CashitoBackend.Vehicles.Domain.Model.Aggregates;
using CashitoBackend.Vehicles.Domain.Model.Queries;
using CashitoBackend.Vehicles.Domain.Repositories;
using CashitoBackend.Vehicles.Domain.Services;

namespace CashitoBackend.Vehicles.Application.Internal.QueryServices;

public class VehicleQueryService : IVehicleQueryService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleQueryService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }
    
    public async Task<IEnumerable<Vehicle>> Handle(GetAllVehiclesQuery query)
    {
        return await _vehicleRepository.FindByUserIdAsync(query.UserId);
    }
    
    public async Task<Vehicle?> Handle(GetVehicleByIdQuery query)
    {
        var vehicle = await _vehicleRepository.FindByIdAsync(query.Id);

        if (vehicle == null)
            return null;

        if (vehicle.UserId != query.UserId)
            return null;

        return vehicle;
    }
}