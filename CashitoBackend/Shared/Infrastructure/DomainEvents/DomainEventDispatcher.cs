using CashitoBackend.Shared.Domain.Events;

namespace CashitoBackend.Shared.Infrastructure.DomainEvents;


public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            if (!handlers.Any())
            {
                _logger.LogDebug("No handlers registered for domain event {EventType}", domainEvent.GetType().Name);
                continue;
            }

            var handleMethod = handlerType.GetMethod("HandleAsync");
            
            foreach (var handler in handlers)
            {
                
                if (handleMethod == null) continue;

                var task = (Task?)handleMethod.Invoke(handler, new object?[] { domainEvent, cancellationToken });
                if (task is not null)
                    await task.ConfigureAwait(false);
            }
        }
    }
} 