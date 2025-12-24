using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.Profiles.Integrations.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Profiles.Integrations.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the ProfilesIntegrationService and its dependencies.
    /// </summary>
    public static IServiceCollection AddProfilesIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // Register AttendrConfiguration for base URL
        services.AddOptions<AttendrConfiguration>()
            .BindConfiguration(AttendrConfiguration.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.Integration.Profiles), "Attendr:Integration:Profiles is required");

        // Register ProfilesIntegrationOptions for cache TTL
        services.AddOptions<ProfilesIntegrationOptions>()
            .BindConfiguration(ProfilesIntegrationOptions.SectionName);

        // Typed HttpClient with base address from AttendrConfiguration
        services.AddHttpClient<IProfilesIntegrationService, ProfilesIntegrationService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<AttendrConfiguration>>().Value;
            client.BaseAddress = new Uri(config.Integration.Profiles);
        });

        return services;
    }
}
