namespace PS.OrderEvents.Domain.Events
{
    public interface IDomainEvent
    {
        public DateTime OccurredOn { get;}
    }
}
