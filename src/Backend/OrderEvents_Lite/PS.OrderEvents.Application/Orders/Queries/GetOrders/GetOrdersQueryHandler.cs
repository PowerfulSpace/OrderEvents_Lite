using MediatR;
using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Application.Orders.DTO;

namespace PS.OrderEvents.Application.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _repository;

    public GetOrdersQueryHandler(IOrderRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        var orders = await _repository.GetAllAsync(ct);
        return orders.Select(o => new OrderDto(o.Id, o.Status, o.CreatedAt)).ToList();
    }
}