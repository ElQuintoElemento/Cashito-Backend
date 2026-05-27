using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Domain.Repositories;
using CashitoBackend.Credits.Domain.Services;
using CashitoBackend.Shared.Domain.Repositories;

namespace CashitoBackend.Credits.Application.Internal.CommandServices;

public class CreditPublicService : ICreditPublicService
{
    private readonly ICreditRepository _creditRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreditNotificationService _notificationService;

    public CreditPublicService(
        ICreditRepository creditRepository,
        IUnitOfWork unitOfWork,
        CreditNotificationService notificationService)
    {
        _creditRepository = creditRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Credit?> GetCreditAsync(int creditId, string token)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);
        return IsTokenValid(credit, token) ? credit : null;
    }

    public async Task<IReadOnlyList<Installment>?> GetScheduleAsync(int creditId, string token)
    {
        var credit = await _creditRepository.FindByIdWithScheduleAsync(creditId);

        if (!IsTokenValid(credit, token))
            return null;

        return credit!.Schedule;
    }

    public async Task<bool> PayInstallmentAsync(int creditId, int installmentNumber, string token)
    {
        var credit = await _creditRepository.FindByIdWithScheduleAsync(creditId);

        if (!IsTokenValid(credit, token))
            return false;

        var paidInstallment = credit!.Schedule.FirstOrDefault(i => i.Number == installmentNumber);
        credit.PayInstallment(installmentNumber);

        _creditRepository.Update(credit);
        await _unitOfWork.CompleteAsync();

        var amount = paidInstallment?.TotalPayment ?? 0;
        await _notificationService.OnInstallmentPaidAsync(credit, installmentNumber, amount);

        if (credit.Schedule.All(i => i.IsPaid))
            await _notificationService.OnCreditCompletedAsync(credit);

        return true;
    }

    public async Task<bool> ApproveAsync(int creditId, string token)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (!IsTokenValid(credit, token))
            return false;

        credit!.Approve();

        _creditRepository.Update(credit);
        await _unitOfWork.CompleteAsync();
        await _notificationService.OnCreditApprovedAsync(credit);

        return true;
    }

    public async Task<bool> RejectAsync(int creditId, string token)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (!IsTokenValid(credit, token))
            return false;

        credit!.Reject();

        _creditRepository.Update(credit);
        await _unitOfWork.CompleteAsync();
        await _notificationService.OnCreditRejectedAsync(credit);

        return true;
    }

    private static bool IsTokenValid(Credit? credit, string token)
    {
        if (credit == null || string.IsNullOrWhiteSpace(token))
            return false;

        return string.Equals(credit.PublicToken, token, StringComparison.Ordinal);
    }
}
