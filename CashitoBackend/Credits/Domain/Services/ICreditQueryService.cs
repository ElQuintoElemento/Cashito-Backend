using CashitoBackend.Credits.Domain.Model.Aggregates;
using CashitoBackend.Credits.Domain.Model.Entities;
using CashitoBackend.Credits.Domain.Model.Queries;
using CashitoBackend.Credits.Domain.Model.ValueObjects;

namespace CashitoBackend.Credits.Domain.Services;

public interface ICreditQueryService
{
    Task<IEnumerable<Credit>> Handle(GetAllCreditsQuery query);

    Task<Credit?> Handle(GetCreditByIdQuery query);

    Task<IEnumerable<Installment>> Handle(GetCreditScheduleQuery query);

    Task<IEnumerable<Credit>> Handle(GetCreditsByStatusQuery query);
}