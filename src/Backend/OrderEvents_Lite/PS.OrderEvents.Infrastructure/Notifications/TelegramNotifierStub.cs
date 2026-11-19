using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PS.OrderEvents.Application.Abstractions;

namespace PS.OrderEvents.Infrastructure.Notifications;

public sealed class TelegramNotifierStub(
    IConfiguration configuration,
    ILogger<TelegramNotifierStub> logger) : ITelegramNotifier
{
    public async Task SendOrderCreatedAsync(Guid orderId, CancellationToken ct = default)
    {
        // Stub. No real HTTP call. Replace with real Bot API call later.
        if (configuration.GetValue<bool>("Telegram:SimulateFailure"))
            throw new InvalidOperationException("Telegram is unavailable (simulated).");

        logger.LogInformation("[TELEGRAM] Would send notification for order {OrderId}", orderId);
        await Task.CompletedTask;
    }
}
