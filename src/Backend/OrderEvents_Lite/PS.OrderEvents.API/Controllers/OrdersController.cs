using MediatR;
using Microsoft.AspNetCore.Mvc;
using PS.OrderEvents.Application.Orders.Commands.ChangeOrderStatus;
using PS.OrderEvents.Application.Orders.Commands.CreateOrder;
using PS.OrderEvents.Application.Orders.DTO;
using PS.OrderEvents.Application.Orders.Queries.GetOrder;
using PS.OrderEvents.Application.Orders.Queries.GetOrders;
using PS.OrderEvents.Domain.Enums;

namespace PS.OrderEvents.API.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var id = await mediator.Send(new CreateOrderCommand(), ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeOrderStatusRequest request, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new ChangeOrderStatusCommand(id, request.NewStatus), ct);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var order = await mediator.Send(new GetOrderQuery(id), ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(CancellationToken ct)
    {
        var orders = await mediator.Send(new GetOrdersQuery(), ct);
        return Ok(orders);
    }
}

public sealed record ChangeOrderStatusRequest(OrderStatus NewStatus);
