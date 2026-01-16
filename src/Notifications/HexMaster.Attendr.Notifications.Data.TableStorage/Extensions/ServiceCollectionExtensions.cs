using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Data.TableStorage.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Extensions;

/// <summary>
/// Extension methods for configuring Azure Table Storage services for notifications.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Table Storage repositories for notifications to the service collection.
    /// </summary>
    public static IServiceCollection AddTableStorageNotificationRepositories(this IServiceCollection services)
    {
        services.AddScoped<INotificationRepository, TableStorageNotificationRepository>();
        services.AddScoped<INotificationPreferencesRepository, TableStorageNotificationPreferencesRepository>();
        services.AddScoped<IPushSubscriptionRepository, TableStoragePushSubscriptionRepository>();

        return services;
    }
}
