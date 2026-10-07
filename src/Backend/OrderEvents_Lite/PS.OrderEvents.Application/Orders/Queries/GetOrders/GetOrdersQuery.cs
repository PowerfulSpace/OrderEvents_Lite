using MediatR;
using PS.OrderEvents.Application.Orders.DTO;

namespace PS.OrderEvents.Application.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery : IRequest<IReadOnlyList<OrderDto>>;