using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PS.OrderEvents.Application.Abstractions;

namespace PS.OrderEvents.Infrastructure.Events;

public sealed class DomainEventQueueProcessor(
    InMemoryDomainEventQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<DomainEventQueueProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var domainEvent in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await Task.Delay(3000, stoppingToken); // demo delay, remove later

                using var scope = scopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
                await dispatcher.DispatchAsync([domainEvent], stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process domain event {EventType}", domainEvent.GetType().Name);
            }
        }
    }
}
