// Infrastructure/Events/DomainEventDispatcher.cs
using Microsoft.Extensions.DependencyInjection;
using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Infrastructure.Events;

public sealed class DomainEventDispatcher(IServiceProvider sp) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        // 1. Достать ВСЕХ подписчиков из DI — один раз, вне цикла
        var handlers = sp.GetServices<IEventHandler>().ToList();

        // 2. Раздать каждое событие тем, кому оно адресовано
        foreach (var domainEvent in domainEvents)
            foreach (var handler in handlers)
                if (handler.CanHandle(domainEvent))          // "твоё?"
                    await handler.HandleAsync(domainEvent, ct);  // доставить (мост → typed)
    }
}