using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Domain.Model.Queries;
using CashitoBackend.Credits.Domain.Repositories;
using CashitoBackend.Credits.Domain.Services;

namespace CashitoBackend.Credits.Application.Internal.QueryServices;

public class CreditQueryService : ICreditQueryService
{
    private readonly ICreditRepository _creditRepository;

    public CreditQueryService(ICreditRepository creditRepository)
    {
        _creditRepository = creditRepository;
    }

    public async Task<IEnumerable<Credit>> Handle(GetAllCreditsQuery query)
    {
        return await _creditRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<Credit?> Handle(GetCreditByIdQuery query)
    {
        var credit = await _creditRepository.FindByIdAsync(query.Id);

        if (credit == null || credit.UserId != query.UserId)
            return null;

        return credit;
    }

    public async Task<IEnumerable<Installment>> Handle(GetCreditScheduleQuery query)
    {
        var credit = await _creditRepository.FindByIdWithScheduleAsync(query.CreditId);

        if (credit == null || credit.UserId != query.UserId)
            return Enumerable.Empty<Installment>();

        return credit.Schedule;
    }

    public async Task<IEnumerable<Credit>> Handle(GetCreditsByStatusQuery query)
    {
        return await _creditRepository
            .FindByUserIdAndStatusAsync(query.UserId, query.Status);
    }
}