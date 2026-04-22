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
    
    public void Update(Client client)
    {
        _context.Clients.Update(client);
    }
    
    public void Remove(Client client)
    {
        _context.Clients.Remove(client);
    }
}