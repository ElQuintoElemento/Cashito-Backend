using CashitoBackend.Credits.Domain.Model.Aggregates;

namespace CashitoBackend.Credits.Domain.Repositories;

public interface ICreditRepository
{
    Task AddAsync(Credit credit);

    Task<Credit?> FindByIdAsync(int id);

    Task<IEnumerable<Credit>> FindByUserIdAsync(int userId);

    void Update(Credit credit);

    void Remove(Credit credit);
}