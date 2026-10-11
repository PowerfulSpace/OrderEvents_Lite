using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Abstractions;

public interface IEventHandler
{
    bool CanHandle(IDomainEvent domainEvent);
    Task HandleAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}


public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}