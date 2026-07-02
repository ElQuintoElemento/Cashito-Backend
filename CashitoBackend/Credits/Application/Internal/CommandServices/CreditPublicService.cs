using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Domain.Repositories;
using CashitoBackend.Credits.Domain.Services;

namespace CashitoBackend.Credits.Application.Internal.CommandServices;

public class CreditPublicService : ICreditPublicService
{
    private readonly ICreditRepository _creditRepository;
    private readonly ICreditCommandService _creditCommandService;

    public CreditPublicService(
        ICreditRepository creditRepository,
        ICreditCommandService creditCommandService)
    {
        _creditRepository = creditRepository;
        _creditCommandService = creditCommandService;
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
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (!IsTokenValid(credit, token))
            return false;

        return await _creditCommandService.PayInstallment(creditId, installmentNumber, credit!.UserId);
    }

    public async Task<bool> ApproveAsync(int creditId, string token)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (!IsTokenValid(credit, token))
            return false;

        return await _creditCommandService.Approve(creditId, credit!.UserId);
    }

    public async Task<bool> RejectAsync(int creditId, string token)
    {
        var credit = await _creditRepository.FindByIdAsync(creditId);

        if (!IsTokenValid(credit, token))
            return false;

        return await _creditCommandService.Reject(creditId, credit!.UserId);
    }

    private static bool IsTokenValid(Credit? credit, string token)
    {
        if (credit == null || string.IsNullOrWhiteSpace(token))
            return false;

        return string.Equals(credit.PublicToken, token, StringComparison.Ordinal);
    }
}
