using System.Linq;
using CashitoBackend.Shared.Domain.Repositories;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using CashitoBackend.Shared.Domain.Events;
using CashitoBackend.Shared.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     Unit of work for the application.
/// </summary>
/// <remarks>
///     This class is used to save changes to the database context.
///     It implements the IUnitOfWork interface.
/// </remarks>
/// <param name="context">
///     The database context for the application
/// </param>
/// <param name="dispatcher">
///     The domain event dispatcher for the application
/// </param>
public class UnitOfWork(AppDbContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    // inheritedDoc
    public async Task CompleteAsync()
    {
        // Persistir cambios primero
        await context.SaveChangesAsync();

        // Recopilar y publicar eventos de dominio
        var domainEntities = context.ChangeTracker
            .Entries<AuditableAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Limpiar antes de publicar para evitar duplicados en caso de fallo posterior
        domainEntities.ForEach(e => e.ClearDomainEvents());

        if (domainEvents.Count > 0)
            await dispatcher.DispatchAsync(domainEvents);
    }
}