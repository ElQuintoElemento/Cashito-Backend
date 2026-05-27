using CashitoBackend.Vehicles.Domain.Model.Aggregates;

namespace CashitoBackend.Vehicles.Domain.Repositories;

public interface IVehicleRepository
{
    Task AddAsync(Vehicle vehicle);

    Task<Vehicle?> FindByIdAsync(int id);

    Task<IEnumerable<Vehicle>> FindByUserIdAsync(int userId);

    Task<int> CountByUserIdAsync(int userId);

    Task<IReadOnlyList<Vehicle>> FindRecentByUserIdAsync(int userId, int limit);

    void Update(Vehicle vehicle);

    void Remove(Vehicle vehicle);
}