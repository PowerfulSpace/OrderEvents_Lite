using Microsoft.EntityFrameworkCore;
using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Entities;

namespace PS.OrderEvents.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(OrderEventsDbContext context) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<List<Order>> GetAllAsync(CancellationToken ct = default)
        => await context.Orders.ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
        => await context.Orders.AddAsync(order, ct);
}