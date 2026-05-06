using CashitoBackend.Credits.Domain.Model.ValueObjects;

namespace CashitoBackend.Credits.Domain.Model.Queries;

public record GetCreditsByStatusQuery(int UserId, CreditStatus Status);