using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Abstractions;

public interface IDomainEventQueue
{
    ValueTask EnqueueAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
