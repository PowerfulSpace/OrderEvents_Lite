using PS.OrderEvents.Application.Abstractions;
using PS.OrderEvents.Infrastructure.Notifications;

namespace PS.OrderEvents.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITelegramNotifier, TelegramNotifierStub>();

        return services;
    }
}
