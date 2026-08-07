using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Entities;

namespace CashitoBackend.Credits.Domain.Services;

public interface ICreditPublicService
{
    Task<Credit?> GetCreditAsync(int creditId, string token);

    Task<IReadOnlyList<Installment>?> GetScheduleAsync(int creditId, string token);

    Task<bool> PayInstallmentAsync(int creditId, int installmentNumber, string token);

    Task<bool> ApproveAsync(int creditId, string token);

    Task<bool> RejectAsync(int creditId, string token);
}
