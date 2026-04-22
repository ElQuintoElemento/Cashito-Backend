using CashitoBackend.Clients.Domain.Model.Aggregates;

namespace CashitoBackend.Clients.Domain.Repositories;

public interface IClientRepository
{
    Task AddAsync(Client client);

    Task<Client?> FindByIdAsync(int id);

    Task<IEnumerable<Client>> FindByUserIdAsync(int userId);

    void Update(Client client);

    void Remove(Client client);
}