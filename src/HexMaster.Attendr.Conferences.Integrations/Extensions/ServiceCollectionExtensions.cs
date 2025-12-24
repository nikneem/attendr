using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Conferences.Integrations.Services;
using HexMaster.Attendr.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Conferences.Integrations.Extensions;

/// <summary>
/// Extension methods for registering Conferences Integration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Conferences Integration service with HTTP client and caching.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConferencesIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configure options
        services.AddOptions<AttendrConfiguration>()
            .BindConfiguration(AttendrConfiguration.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.Integration.Conferences), "Attendr:Integration:Conferences is required");

        // Register typed HttpClient with base address from options
        services.AddHttpClient<IConferencesIntegrationService, ConferencesIntegrationService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<AttendrConfiguration>>().Value;
            client.BaseAddress = new Uri(config.Integration.Conferences);
        });

        return services;
    }
}
