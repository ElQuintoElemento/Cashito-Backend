using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Repositories;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.Credits.Infrastructure.Persistence.EFC.Repositories;


public class CreditRepository : ICreditRepository
{
    private readonly AppDbContext _context;

    public CreditRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(Credit credit)
    {
        await  _context.Credits.AddAsync(credit);
    }
    
    public async Task<Credit?> FindByIdAsync(int id)
    {
        return await _context.Credits
            .Include(c => c.Schedule)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<IEnumerable<Credit>> FindByUserIdAsync(int userId)
    {
        return await  _context.Credits
            .Where(c => c.UserId == userId)
            .Include(c => c.Schedule)
            .ToListAsync();
    }
    
    public void Update(Credit credit)
    {
        _context.Credits.Update(credit);
    }
    
    public void Remove(Credit credit)
    {
        _context.Credits.Remove(credit);
    }
}