using HexMaster.Attendr.Profiles.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Profiles.Data.TableStorage.Extensions;

/// <summary>
/// Extension methods for configuring Azure Table Storage services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Table Storage services and the profile repository to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTableStorageProfileRepository(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IProfileRepository, TableStorageProfileRepository>();
        services.AddScoped<IProfileTopicRepository, TableStorageProfileTopicRepository>();

        return services;
    }
}
