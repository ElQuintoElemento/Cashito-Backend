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

    // 🔹 SIMULACIÓN
    public async Task<SimulationResult> Handle(SimulateCreditCommand command, int userId)
    {
        return _simulationService.Simulate(command);
    }

    // 🔹 CREAR CRÉDITO
    public async Task<Credit> Handle(CreateCreditCommand command, int userId)
    {
        var simulateCommand = new SimulateCreditCommand(
            command.ClientId,
            command.VehicleId,
            command.VehiclePrice,
            command.Currency,
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
            command.Currency,
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

    // 🔹 APPROVE
    public async Task<bool> Approve(int creditId, int userId)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (credit == null || credit.UserId != userId)
            return false;

        credit.Approve();

        _creditRepository.Update(credit);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    // 🔹 ACTIVATE
    public async Task<bool> Activate(int creditId, int userId)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (credit == null || credit.UserId != userId)
            return false;

        credit.Activate();

        _creditRepository.Update(credit);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    // 🔹 REJECT
    public async Task<bool> Reject(int creditId, int userId)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (credit == null || credit.UserId != userId)
            return false;

        credit.Reject();

        _creditRepository.Update(credit);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    // 🔹 COMPLETE
    public async Task<bool> Complete(int creditId, int userId)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (credit == null || credit.UserId != userId)
            return false;

        credit.Complete();

        _creditRepository.Update(credit);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    // 🔹 PAGAR CUOTA
    public async Task<bool> PayInstallment(int creditId, int installmentNumber, int userId)
    {
        var credit = await _creditRepository.FindByIdWithScheduleAsync(creditId);

        if (credit == null || credit.UserId != userId)
            return false;

        credit.PayInstallment(installmentNumber);

        _creditRepository.Update(credit);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    // 🔹 DELETE
    public async Task<bool> Delete(int creditId, int userId)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (credit == null || credit.UserId != userId)
            return false;

        _creditRepository.Remove(credit);
        await _unitOfWork.CompleteAsync();

        return true;
    }
}