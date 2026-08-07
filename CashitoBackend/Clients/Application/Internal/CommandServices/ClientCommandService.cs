using CashitoBackend.Clients.Domain.Model.Aggregates;
using CashitoBackend.Clients.Domain.Model.Commands;
using CashitoBackend.Clients.Domain.Repositories;
using CashitoBackend.Clients.Domain.Services;
using CashitoBackend.Shared.Domain.Model.ValueObjects;
using CashitoBackend.Shared.Domain.Repositories;
using CashitoBackend.Clients.Domain.Model.ValueObjects;
using CashitoBackend.Shared.Domain.Exceptions;

namespace CashitoBackend.Clients.Application.Internal.CommandServices;

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
        if (command.MonthlyIncome <= 0)
            throw new BadRequestException("Monthly income must be strictly greater than 0");

        Dni dniVo;
        try
        {
            dniVo = new Dni(command.Dni);
        }
        catch (ArgumentException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        if (await _clientRepository.ExistsByDniAsync(dniVo))
            throw new BadRequestException("DNI already exists");

        if (!string.IsNullOrWhiteSpace(command.Phone))
        {
            try
            {
                _ = new PhoneNumber(command.Phone);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        Client client;
        try
        {
            client = new Client(
                userId,
                command.Dni,
                command.FirstName,
                command.LastName,
                command.MonthlyIncome,
                command.IncomeCurrency,
                command.Phone,
                new EmailAddress(command.Email)
            );
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }

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

        if (command.MonthlyIncome <= 0)
            throw new BadRequestException("Monthly income must be strictly greater than 0");

        if (!string.IsNullOrWhiteSpace(command.Phone))
        {
            try
            {
                _ = new PhoneNumber(command.Phone);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        try
        {
            client.Update(
                command.FirstName,
                command.LastName,
                command.MonthlyIncome,
                command.IncomeCurrency,
                command.Phone,
                new EmailAddress(command.Email)
            );
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }

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