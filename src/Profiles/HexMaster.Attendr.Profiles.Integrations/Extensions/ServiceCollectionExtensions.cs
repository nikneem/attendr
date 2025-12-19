using HexMaster.Attendr.Core.Cache;
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

        services.AddOptions<ProfilesIntegrationOptions>()
            .BindConfiguration(ProfilesIntegrationOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "Profiles Integration BaseUrl is required");

        // Typed HttpClient with base address from options
        services.AddHttpClient<IProfilesIntegrationService, ProfilesIntegrationService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<ProfilesIntegrationOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
        });

        // Ensure cache client is available (registered elsewhere via AddAttendrCache)
        services.AddOptions();

        return services;
    }
}
