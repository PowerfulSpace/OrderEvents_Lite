using MediatR;
using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Application.Orders.DTO;

namespace PS.OrderEvents.Application.Orders.Queries.GetOrder;

public sealed class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderRepository _repository;

    public GetOrderQueryHandler(IOrderRepository repository) => _repository = repository;

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(request.OrderId, ct);
        return order is null ? null : new OrderDto(order.Id, order.Status, order.CreatedAt);
    }
}