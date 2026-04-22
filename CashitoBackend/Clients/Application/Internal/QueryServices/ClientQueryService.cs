using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Domain.Model.Queries;
using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Clients.Domain.Services;

namespace CashitoBackend.Clients.Application.Internal.QueryServices;

public class ClientQueryService : IClientQueryService
{
    private readonly IClientRepository _clientRepository;

    public ClientQueryService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }
    
    public async Task<IEnumerable<Client>> Handle(GetAllClientsQuery query)
    {
        return await _clientRepository.FindByUserIdAsync(query.UserId);
    }
    
    public async Task<Client?> Handle(GetClientByIdQuery query)
    {
        var client = await _clientRepository.FindByIdAsync(query.Id);

        if (client == null)
            return null;

        if (client.UserId != query.UserId)
            return null;

        return client;
    }
}