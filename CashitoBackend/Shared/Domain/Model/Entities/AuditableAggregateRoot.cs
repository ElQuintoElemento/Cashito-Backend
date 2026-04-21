using CashitoBackend.Shared.Domain.Events;

namespace CashitoBackend.Shared.Domain.Model.Entities;

public abstract class AuditableAggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}