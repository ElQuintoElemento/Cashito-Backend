using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.ValueObjects;

namespace CashitoBackend.Credits.Domain.Repositories;

public interface ICreditRepository
{
    Task AddAsync(Credit credit);

    Task<Credit?> FindByIdAsync(int id);

    Task<Credit?> FindByIdWithScheduleAsync(int id);

    Task<IEnumerable<Credit>> FindByUserIdAsync(int userId);

    void Update(Credit credit);

    void Remove(Credit credit);
    
    Task<IEnumerable<Credit>> FindByUserIdAndStatusAsync(int userId, CreditStatus status);
}