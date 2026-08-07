using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Domain.Model.Commands;

namespace CashitoBackend.Clients.Domain.Services;

public interface IClientCommandService
{
    Task<Client> Handle(CreateClientCommand command, int userId);

    Task<Client?> Handle(UpdateClientCommand command, int userId);

    Task<bool> Handle(DeleteClientCommand command, int userId);
}