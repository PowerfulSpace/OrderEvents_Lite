using MediatR;
using PS.OrderEvents.Application.Abstractions;

namespace PS.OrderEvents.Application.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusCommandHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeOrderStatusCommand, Unit>
{
    public async Task<Unit> Handle(ChangeOrderStatusCommand request, CancellationToken ct)
    {
        var order = await repository.GetByIdAsync(request.OrderId, ct)
            ?? throw new InvalidOperationException($"Order {request.OrderId} not found");

        order.ChangeStatus(request.NewStatus);

        await unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}