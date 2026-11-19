using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Application.Events;

namespace PS.OrderEvents.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler, OrderCreatedHandler>();
        services.AddScoped<IEventHandler, OrderStatusChangedHandler>();
        return services;
    }
}
