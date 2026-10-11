using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Infrastructure.Events;
using PS.OrderEvents.Infrastructure.Persistence;
using PS.OrderEvents.Infrastructure.Persistence.Repositories;

namespace PS.OrderEvents.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not configured");

        services.AddDbContext<OrderEventsDbContext>(opt => opt.UseSqlite(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}