using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Abstractions;

public abstract class DomainEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    public abstract Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);

    public bool CanHandle(IDomainEvent domainEvent) => domainEvent is TEvent;

    Task IEventHandler.HandleAsync(IDomainEvent domainEvent, CancellationToken ct = default)
        => HandleAsync((TEvent)domainEvent, ct);
}