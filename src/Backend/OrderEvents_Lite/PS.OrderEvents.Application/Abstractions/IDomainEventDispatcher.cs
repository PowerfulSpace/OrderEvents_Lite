using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Abstractions
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
    }
}
