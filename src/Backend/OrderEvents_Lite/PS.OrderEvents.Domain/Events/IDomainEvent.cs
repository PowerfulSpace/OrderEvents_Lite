using MediatR;

namespace PS.OrderEvents.Domain.Events;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
