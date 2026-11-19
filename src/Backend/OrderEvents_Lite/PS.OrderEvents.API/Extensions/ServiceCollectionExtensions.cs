using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Application.Events;
using PS.OrderEvents.Infrastructure.Notifications;

namespace PS.OrderEvents.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler, OrderCreatedHandler>();
        services.AddScoped<IEventHandler, OrderStatusChangedHandler>();

        services.AddScoped<ITelegramNotifier, TelegramNotifierStub>();
        services.AddScoped<IEventHandler, TelegramOrderCreatedHandler>();

        return services;
    }
}
