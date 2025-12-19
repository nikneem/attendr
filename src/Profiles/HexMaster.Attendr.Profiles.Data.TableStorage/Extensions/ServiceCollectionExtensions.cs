using Azure.Data.Tables;
using HexMaster.Attendr.Profiles.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Profiles.Data.TableStorage.Extensions;

/// <summary>
/// DI helpers to register the Azure Table Storage repository for profiles.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProfilesTableStorage(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<TableStorageProfilesOptions>()
            .BindConfiguration(TableStorageProfilesOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "ConnectionString is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.TableName), "TableName is required");

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<TableStorageProfilesOptions>>().Value;
            var tableServiceClient = new TableServiceClient(options.ConnectionString);
            var tableClient = tableServiceClient.GetTableClient(options.TableName);
            tableClient.CreateIfNotExists();
            return tableClient;
        });

        services.AddScoped<IProfileRepository, TableStorageProfileRepository>();

        return services;
    }
}
