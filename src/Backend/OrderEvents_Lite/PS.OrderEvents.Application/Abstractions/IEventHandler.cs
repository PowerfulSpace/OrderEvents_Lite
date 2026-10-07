using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Abstractions;

public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}