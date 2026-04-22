using CashitoBackend.Vehicles.Domain.Model.Aggregates;

namespace CashitoBackend.Vehicles.Domain.Repositories;

public interface IVehicleRepository
{
    Task AddAsync(Vehicle vehicle);

    Task<Vehicle?> FindByIdAsync(int id);

    Task<IEnumerable<Vehicle>> FindByUserIdAsync(int userId);

    void Update(Vehicle vehicle);

    void Remove(Vehicle vehicle);
}