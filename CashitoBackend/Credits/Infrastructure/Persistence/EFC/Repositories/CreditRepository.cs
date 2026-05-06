using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.ValueObjects;
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

    // =========================
    // CREATE
    // =========================
    public async Task AddAsync(Credit credit)
    {
        await _context.Credits.AddAsync(credit);
    }

    // =========================
    // READ (LIGHT)
    // =========================
    public async Task<Credit?> FindByIdAsync(int id)
    {
        return await _context.Credits
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // =========================
    // READ (FULL WITH SCHEDULE)
    // =========================
    public async Task<Credit?> FindByIdWithScheduleAsync(int id)
    {
        return await _context.Credits
            .Include(c => c.Schedule)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // =========================
    // LIST BY USER (LIGHT)
    // =========================
    public async Task<IEnumerable<Credit>> FindByUserIdAsync(int userId)
    {
        return await _context.Credits
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    // =========================
    // LIST BY USER + STATUS
    // =========================
    public async Task<IEnumerable<Credit>> FindByUserIdAndStatusAsync(int userId, CreditStatus status)
    {
        return await _context.Credits
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.Status == status)
            .ToListAsync();
    }

    // =========================
    // UPDATE
    // =========================
    public void Update(Credit credit)
    {
        _context.Credits.Update(credit);
    }

    // =========================
    // DELETE
    // =========================
    public void Remove(Credit credit)
    {
        _context.Credits.Remove(credit);
    }
}