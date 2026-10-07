using MediatR;
using PS.OrderEvents.Application.Orders.DTO;

namespace PS.OrderEvents.Application.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>;