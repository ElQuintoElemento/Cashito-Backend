namespace CashitoBackend.Shared.Domain.Model.Entities;

public abstract class AuditableEntity
{
    public long Id { get; protected set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void SetCreated(DateTime when) => CreatedAt = when;
    public void SetUpdated(DateTime when) => UpdatedAt = when;
} 