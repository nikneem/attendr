using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HexMaster.Attendr.Groups.Data.MongoDb.Extensions;

/// <summary>
/// Extension methods for configuring MongoDB services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MongoDB services and the group repository to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMongoDbGroupRepository(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);


        Console.WriteLine(Environment.GetEnvironmentVariable("MongoDb__ConnectionString"));

        // Configure options
        services.AddOptions<MongoDbOptions>()
            .BindConfiguration(MongoDbOptions.SectionName)
            .ValidateOnStart()
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "ConnectionString is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.DatabaseName), "DatabaseName is required")
            .PostConfigure<IConfiguration>((options, config) =>
            {
                // Log configuration for debugging
                Console.WriteLine($"MongoDb Configuration - ConnectionString: {(string.IsNullOrEmpty(options.ConnectionString) ? "NOT SET" : "SET")}");
                Console.WriteLine($"MongoDb Configuration - DatabaseName: {options.DatabaseName}");
            });

        // Register MongoDB client
        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            return new MongoClient(options.ConnectionString);
        });

        // Register MongoDB database
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            return client.GetDatabase(options.DatabaseName);
        });

        // Register repository
        services.AddSingleton<IGroupRepository, MongoDbGroupRepository>();

        return services;
    }
}
