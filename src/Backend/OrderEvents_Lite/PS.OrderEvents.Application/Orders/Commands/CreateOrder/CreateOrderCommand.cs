using MediatR;

namespace PS.OrderEvents.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand : IRequest<Guid>;