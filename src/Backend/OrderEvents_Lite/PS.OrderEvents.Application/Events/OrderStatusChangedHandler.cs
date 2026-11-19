using MediatR;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Events;

public sealed class OrderStatusChangedHandler : INotificationHandler<OrderStatusChanged>
{
    public Task Handle(OrderStatusChanged notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[EVENT] Order {notification.OrderId}: {notification.OldStatus} → {notification.NewStatus}");
        return Task.CompletedTask;
    }
}
