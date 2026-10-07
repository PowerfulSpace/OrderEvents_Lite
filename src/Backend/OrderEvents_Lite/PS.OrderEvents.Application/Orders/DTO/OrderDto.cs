using PS.OrderEvents.Domain.Enums;

namespace PS.OrderEvents.Application.Orders.DTO;

public sealed record OrderDto(Guid Id, OrderStatus Status, DateTime CreatedAt);