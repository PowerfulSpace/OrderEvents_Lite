using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Events;

public sealed class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated domainEvent, CancellationToken ct = default)
    {
        Console.WriteLine($"[EVENT] Order created: {domainEvent.OrderId}");
        return Task.CompletedTask;
    }
}