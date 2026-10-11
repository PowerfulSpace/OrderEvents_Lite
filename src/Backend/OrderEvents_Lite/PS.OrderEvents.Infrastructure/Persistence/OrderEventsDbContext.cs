using Microsoft.EntityFrameworkCore;
using PS.OrderEvents.Domain.Entities;

namespace PS.OrderEvents.Infrastructure.Persistence;

public class OrderEventsDbContext(DbContextOptions<OrderEventsDbContext> options)
    : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderEventsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}