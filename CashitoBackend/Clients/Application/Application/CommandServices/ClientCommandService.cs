using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Domain.Model.Commands;
using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Clients.Domain.Services;
using CashitoBackend.Shared.Domain.Repositories;

namespace CashitoBackend.Clients.Application.Application.CommandServices;

public class ClientCommandService : IClientCommandService
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientCommandService(
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Client> Handle(CreateClientCommand command, int userId)
    {
        var client = new Client(
            userId,
            command.Dni,
            command.FirstName,
            command.LastName,
            command.MonthlyIncome,
            command.Phone
        );

        await _clientRepository.AddAsync(client);
        await _unitOfWork.CompleteAsync();

        return client;
    }
    
    public async Task<Client?> Handle(UpdateClientCommand command, int userId)
    {
        var client = await _clientRepository.FindByIdAsync(command.Id);

        if (client == null)
            return null;

        if (client.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed");

        client.Update(
            command.FirstName,
            command.LastName,
            command.MonthlyIncome,
            command.Phone
        );

        _clientRepository.Update(client);
        await _unitOfWork.CompleteAsync();

        return client;
    }
    
    public async Task<bool> Handle(DeleteClientCommand command, int userId)
    {
        var client = await _clientRepository.FindByIdAsync(command.Id);

        if (client == null)
            return false;

        if (client.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed");

        _clientRepository.Remove(client);
        await _unitOfWork.CompleteAsync();

        return true;
    }
}