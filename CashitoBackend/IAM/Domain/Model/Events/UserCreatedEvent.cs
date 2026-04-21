using CashitoBackend.Shared.Domain.Events;

namespace CashitoBackend.IAM.Domain.Model.Events;

public record UserCreatedEvent(int UserId, string Username) : IDomainEvent; 