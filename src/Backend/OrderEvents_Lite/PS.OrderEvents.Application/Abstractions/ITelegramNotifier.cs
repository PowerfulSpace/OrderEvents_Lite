namespace PS.OrderEvents.Application.Abstractions;

public interface ITelegramNotifier
{
    Task SendOrderCreatedAsync(Guid orderId, CancellationToken ct = default);
}
