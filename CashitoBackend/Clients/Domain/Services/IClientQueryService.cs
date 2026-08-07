using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Domain.Model.Queries;

namespace CashitoBackend.Clients.Domain.Services;

public interface IClientQueryService
{
    Task<IEnumerable<Client>> Handle(GetAllClientsQuery query);

    Task<Client?> Handle(GetClientByIdQuery query);
}