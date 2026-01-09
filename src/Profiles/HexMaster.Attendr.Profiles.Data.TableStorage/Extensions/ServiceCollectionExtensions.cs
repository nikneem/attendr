using Azure.Data.Tables;
using HexMaster.Attendr.Profiles.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    public static IServiceCollection AddTableStorageProfileRepository(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure options
        services.AddOptions<TableStorageOptions>()
            .BindConfiguration(TableStorageOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "ConnectionString is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.TableName), "TableName is required");

        // Register TableServiceClient
        services.AddSingleton<TableServiceClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TableStorageOptions>>().Value;
            return new TableServiceClient(options.ConnectionString);
        });

        // Register repository
        services.AddSingleton<IProfileRepository>(sp =>
        {
            var tableServiceClient = sp.GetRequiredService<TableServiceClient>();
            var options = sp.GetRequiredService<IOptions<TableStorageOptions>>().Value;
            return new TableStorageProfileRepository(tableServiceClient, options);
        });

        return services;
    }
}
