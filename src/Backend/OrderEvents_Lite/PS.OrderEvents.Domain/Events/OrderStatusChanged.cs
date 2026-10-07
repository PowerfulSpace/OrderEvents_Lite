using PS.OrderEvents.Domain.Enums;

namespace PS.OrderEvents.Domain.Events
{
    public sealed record OrderStatusChanged(
        Guid OrderId,
        OrderStatus OldStatus,
        OrderStatus NewStatus,
        DateTime OccurredOn
        ) : IDomainEvent;
}
