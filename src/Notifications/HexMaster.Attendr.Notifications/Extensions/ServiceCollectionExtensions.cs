using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.Features.ProcessNotificationTrigger;
using HexMaster.Attendr.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Notifications.Extensions;

/// <summary>
/// Extension methods for configuring notification services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds notification feature services to the service collection.
    /// </summary>
    public static IServiceCollection AddNotificationFeatures(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<INotificationTypeService, NotificationTypeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddHttpClient<IPushNotificationService, PushNotificationService>();

        // Register channel services
        services.AddScoped<INotificationPreferencesCacheService, NotificationPreferencesCacheService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        // Register memory cache for preferences caching
        services.AddMemoryCache();

        // Register command handlers
        services.AddScoped<ICommandHandler<ProcessNotificationTriggerCommand>, ProcessNotificationTriggerCommandHandler>();

        return services;
    }
}
