using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.Clients.Infrastructure.Persistence.EFC.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(Client client)
    {
        await _context.Clients.AddAsync(client);
    }
    
    public async Task<Client?> FindByIdAsync(int id)
    {
        return await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<IEnumerable<Client>> FindByUserIdAsync(int userId)
    {
        return await _context.Clients
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(int userId)
    {
        return await _context.Clients
            .CountAsync(c => c.UserId == userId);
    }

    public async Task<IReadOnlyList<Client>> FindRecentByUserIdAsync(int userId, int limit)
    {
        return await _context.Clients
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.Id)
            .Take(limit)
            .ToListAsync();
    }
    
    public void Update(Client client)
    {
        _context.Clients.Update(client);
    }
    
    public void Remove(Client client)
    {
        _context.Clients.Remove(client);
    }

    public async Task<bool> ExistsByDniAsync(CashitoBackend.Clients.Domain.Model.ValueObjects.Dni dni)
    {
        return await _context.Clients
            .AnyAsync(c => c.Dni == dni);
    }
}