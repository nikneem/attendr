using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Presence.Data.Postgres.Extensions;

/// <summary>
/// Extension methods for configuring PostgreSQL services for the Presence domain.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL repository services for presence aggregates to the service collection.
    /// Expects NpgsqlDataSource to be registered via Aspire.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPostgresPresenceRepositories(this IServiceCollection services)
    {
        // Register repositories (expects NpgsqlDataSource to be registered via Aspire)
        services.AddSingleton<IConferencePresenceRepository, PostgresConferencePresenceRepository>();
        services.AddSingleton<IPresentationPresenceRepository>(serviceProvider =>
        {
            _ = serviceProvider.GetRequiredService<IConferencePresenceRepository>();
            var dataSource = serviceProvider.GetRequiredService<Npgsql.NpgsqlDataSource>();
            return new PostgresPresentationPresenceRepository(dataSource);
        });

        return services;
    }
}
