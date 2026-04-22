namespace CashitoBackend.Credits.Domain.Model.Events;

public record CreditCreatedEvent(int CreditId, int UserId);