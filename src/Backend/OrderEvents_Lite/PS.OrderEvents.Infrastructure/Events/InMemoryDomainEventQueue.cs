using System.Threading.Channels;
using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Infrastructure.Events;

public sealed class InMemoryDomainEventQueue : IDomainEventQueue
{
    private readonly Channel<IDomainEvent> _channel = Channel.CreateBounded<IDomainEvent>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    public ChannelReader<IDomainEvent> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(IDomainEvent domainEvent, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(domainEvent, ct);
}
