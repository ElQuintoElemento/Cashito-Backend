using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using CashitoBackend.Vehicles.Domain.Model.Aggregates;
using CashitoBackend.Vehicles.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.Vehicles.Infrastructure.Persistence.EFC.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _context;

    public VehicleRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(Vehicle vehicle)
    {
        await _context.Vehicles.AddAsync(vehicle);
    }
    
    public async Task<Vehicle?> FindByIdAsync(int id)
    {
        return await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id);
    }
    
    public async Task<IEnumerable<Vehicle>> FindByUserIdAsync(int userId)
    {
        return await _context.Vehicles
            .Where(v => v.UserId == userId)
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(int userId)
    {
        return await _context.Vehicles
            .CountAsync(v => v.UserId == userId);
    }

    public async Task<IReadOnlyList<Vehicle>> FindRecentByUserIdAsync(int userId, int limit)
    {
        return await _context.Vehicles
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.Id)
            .Take(limit)
            .ToListAsync();
    }
    
    public void Update(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
    }
    
    public void Remove(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
    }
}