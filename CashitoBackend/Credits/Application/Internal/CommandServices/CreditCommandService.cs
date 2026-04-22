using CashitoBackend.Credits.Application.Internal.DTOs;
using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Commands;
using CashitoBackend.Credits.Domain.Repositories;
using CashitoBackend.Credits.Domain.Services;
using CashitoBackend.Shared.Domain.Repositories;

namespace CashitoBackend.Credits.Application.Internal.CommandServices;

public class CreditCommandService : ICreditCommandService
{
    private readonly ICreditSimulationService _simulationService;
    private readonly ICreditRepository _creditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreditCommandService(
        ICreditRepository creditRepository,
        IUnitOfWork unitOfWork,
        ICreditSimulationService simulationService)
    {
        _creditRepository = creditRepository;
        _unitOfWork = unitOfWork;
        _simulationService = simulationService;
    }
    
    public async Task<SimulationResult> Handle(SimulateCreditCommand command, int userId)
    {
        return _simulationService.Simulate(command);
    }
    
    public async Task<Credit> Handle(CreateCreditCommand command, int userId)
    {
        var simulateCommand = new SimulateCreditCommand(
            command.ClientId,
            command.VehicleId,
            command.VehiclePrice,
            command.DownPayment,
            command.InterestRate,
            command.TermMonths,
            command.RateType,
            command.GracePeriod,
            command.Insurance
        );

        var simulation = _simulationService.Simulate(simulateCommand);

        var credit = new Credit(
            userId,
            command.ClientId,
            command.VehicleId,
            command.VehiclePrice,
            command.DownPayment,
            command.InterestRate,
            command.TermMonths,
            command.RateType,
            command.GracePeriod,
            command.Insurance
        );

        credit.SetResults(simulation.Tcea, simulation.Van, simulation.Tir);
        credit.SetSchedule(simulation.Installments);

        await _creditRepository.AddAsync(credit);
        await _unitOfWork.CompleteAsync();

        return credit;
    }
    
    public async Task<bool> Delete(int creditId, int userId)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (credit == null)
            return false;

        if (credit.UserId != userId)
            throw new UnauthorizedAccessException("Not allowed");

        _creditRepository.Remove(credit);
        await _unitOfWork.CompleteAsync();

        return true;
    }
}