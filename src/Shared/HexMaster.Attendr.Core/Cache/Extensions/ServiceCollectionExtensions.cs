using Dapr.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Core.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrCache(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<AttendrCacheOptions>()
            .BindConfiguration(AttendrCacheOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.StoreName), "StoreName is required");

        services.AddSingleton<IAttendrCacheClient>(sp =>
        {
            var daprClient = sp.GetRequiredService<DaprClient>();
            var opts = sp.GetRequiredService<IOptions<AttendrCacheOptions>>().Value;
            return new AttendrCacheClient(daprClient, opts);
        });

        return services;
    }
}
