using MediatR;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Events;

public sealed class OrderCreatedHandler : INotificationHandler<OrderCreated>
{
    public Task Handle(OrderCreated notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[EVENT] Order created: {notification.OrderId}");
        return Task.CompletedTask;
    }
}
