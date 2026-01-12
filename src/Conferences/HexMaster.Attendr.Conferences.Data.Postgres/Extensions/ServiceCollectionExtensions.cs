using HexMaster.Attendr.Conferences.Data.Postgres.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Conferences.Data.Postgres.Extensions;

/// <summary>
/// Extension methods for configuring PostgreSQL services for the Conferences domain.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL repository services for conferences to the service collection.
    /// Expects NpgsqlDataSource to be registered via Aspire.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPostgresConferenceRepository(this IServiceCollection services)
    {
        // Register repository (expects NpgsqlDataSource to be registered via Aspire)
        services.AddSingleton<IConferenceRepository, PostgresConferenceRepository>();

        return services;
    }

    /// <summary>
    /// Adds database migration services that will run automatically on application startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseMigrationRunner>();
        services.AddHostedService<MigrationHostedService>();

        return services;
    }
}
