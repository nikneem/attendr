using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.IntegrationEvents.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds integration event publishing services to the service collection.
    /// Configures Dapr options from the configuration section "Dapr".
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration instance containing Dapr settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntegrationEvents(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DaprOptions>(configuration.GetSection(DaprOptions.SectionName));
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();

        return services;
    }
}
