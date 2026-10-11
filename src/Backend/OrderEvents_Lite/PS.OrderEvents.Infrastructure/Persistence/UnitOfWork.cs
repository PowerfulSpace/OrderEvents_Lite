using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Domain.Entities;

namespace PS.OrderEvents.Infrastructure.Persistence;

public sealed class UnitOfWork(
    OrderEventsDbContext context,
    IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Коммит — факты становятся "официальными"
        await context.SaveChangesAsync(ct);

        // 2. Собрать агрегаты с событиями (один обход, материализуем!)
        var entitiesWithEvents = context.ChangeTracker
            .Entries<Order>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        // 3. Быстрый выход — 99% вызовов без событий
        if (entitiesWithEvents.Count == 0)
            return;

        // 4. Собрать сами события (ToList обязателен — перед очисткой!)
        var domainEvents = entitiesWithEvents
            .SelectMany(o => o.DomainEvents)
            .ToList();

        // 5. Раздача подписчикам
        await dispatcher.DispatchAsync(domainEvents, ct);

        // 6. Очистить копилки — событие публикуется ровно один раз
        foreach (var order in entitiesWithEvents)
            order.ClearDomainEvents();
    }
}