using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Events;

public sealed class OrderStatusChangedHandler : DomainEventHandler<OrderStatusChanged>
{
    public override Task HandleAsync(OrderStatusChanged domainEvent, CancellationToken ct = default)
    {
        Console.WriteLine($"[EVENT] Order {domainEvent.OrderId}: {domainEvent.OldStatus} → {domainEvent.NewStatus}");
        return Task.CompletedTask;
    }
}