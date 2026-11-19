using MediatR;
using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Events;

public sealed class TelegramOrderCreatedHandler(ITelegramNotifier notifier) : INotificationHandler<OrderCreated>
{
    public async Task Handle(OrderCreated notification, CancellationToken cancellationToken)
    {
        try
        {
            await notifier.SendOrderCreatedAsync(notification.OrderId, cancellationToken);
        }
        catch (Exception ex)
        {
            // В этом проекте — log & swallow, чтобы одно упавшее уведомление не роняло остальные.
            // В проде — retry / dead-letter / outbox. См. KNOWN_SIMPLIFICATIONS.
            Console.WriteLine($"[TELEGRAM ERROR] Failed to notify order {notification.OrderId}: {ex.Message}");
        }
    }
}
