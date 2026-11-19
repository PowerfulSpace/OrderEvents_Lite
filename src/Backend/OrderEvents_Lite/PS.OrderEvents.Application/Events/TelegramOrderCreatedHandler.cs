using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Application.Events;

public sealed class TelegramOrderCreatedHandler(ITelegramNotifier notifier) : DomainEventHandler<OrderCreated>
{
    public override async Task HandleAsync(OrderCreated evt, CancellationToken ct = default)
    {
        try
        {
            await notifier.SendOrderCreatedAsync(evt.OrderId, ct);
        }
        catch (Exception ex)
        {
            // В этом проекте — log & swallow, чтобы одно упавшее уведомление не роняло остальные.
            // В проде — retry / dead-letter / outbox. См. KNOWN_SIMPLIFICATIONS.
            Console.WriteLine($"[TELEGRAM ERROR] Failed to notify order {evt.OrderId}: {ex.Message}");
        }
    }
}
