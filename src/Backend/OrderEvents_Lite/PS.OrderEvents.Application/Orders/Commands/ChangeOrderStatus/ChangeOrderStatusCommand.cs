using MediatR;
using PS.OrderEvents.Domain.Enums;

namespace PS.OrderEvents.Application.Orders.Commands.ChangeOrderStatus;

public sealed record ChangeOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest<Unit>;