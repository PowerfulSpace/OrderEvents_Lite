namespace PS.OrderEvents.Domain.Events
{
    public sealed record OrderCreated(Guid OrderId, DateTime OccurredOn) : IDomainEvent;

}
