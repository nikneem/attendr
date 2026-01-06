using HexMaster.Attendr.Presence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HexMaster.Attendr.Presence.Data.MongoDb.Extensions;

/// <summary>
/// Extension methods for registering MongoDB repositories in the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers MongoDB-based presence repositories with the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMongoDbPresenceRepository(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<MongoDbOptions>()
            .BindConfiguration(MongoDbOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "ConnectionString is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.DatabaseName), "DatabaseName is required");

        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            return new MongoClient(options.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            return client.GetDatabase(options.DatabaseName);
        });

        services.AddSingleton<IConferencePresenceRepository, ConferencePresenceRepository>();
        services.AddSingleton<IPresentationPresenceRepository, PresentationPresenceRepository>();

        return services;
    }
}
