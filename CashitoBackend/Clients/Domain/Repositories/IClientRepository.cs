using CashitoBackend.Clients.Domain.Model.Aggregates;

namespace CashitoBackend.Clients.Domain.Repositories;

public interface IClientRepository
{
    Task AddAsync(Client client);

    Task<Client?> FindByIdAsync(int id);

    Task<IEnumerable<Client>> FindByUserIdAsync(int userId);

    Task<int> CountByUserIdAsync(int userId);

    Task<IReadOnlyList<Client>> FindRecentByUserIdAsync(int userId, int limit);

    void Update(Client client);

    void Remove(Client client);
}