using PS.OrderEvents.Domain.Enums;
using PS.OrderEvents.Domain.Events;

namespace PS.OrderEvents.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        private Order() { }

        public static Order Create()
        {
            var now = DateTime.UtcNow;

            var order = new Order();
            order.Id = Guid.NewGuid();
            order.Status = OrderStatus.Pending;
            order.CreatedAt = now;
            order.AddDomainEvent(new OrderCreated(order.Id, now));
            return order;
        }

        public void ChangeStatus(OrderStatus newStatus)
        {
            if (Status == newStatus) return;

            var oldStatus = Status;
            Status = newStatus;
            AddDomainEvent(new OrderStatusChanged(Id, oldStatus, newStatus, DateTime.UtcNow));
        }

        private void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
