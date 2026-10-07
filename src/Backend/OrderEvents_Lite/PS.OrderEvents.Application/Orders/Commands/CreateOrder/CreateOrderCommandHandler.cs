using MediatR;
using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Entities;

namespace PS.OrderEvents.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create();

        await repository.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return order.Id;
    }
}