using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Groups.Data.Postgress.Extensions;

/// <summary>
/// Extension methods for configuring PostgreSQL services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL services and the group repository to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPostgresGroupRepository(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure options
        services.AddOptions<PostgresOptions>()
            .BindConfiguration(PostgresOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.SchemaName), "SchemaName is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.TableName), "TableName is required");

        // Register repository (expects NpgsqlDataSource to be registered via Aspire)
        services.AddSingleton<IGroupRepository, PostgresGroupRepository>();

        return services;
    }
}
