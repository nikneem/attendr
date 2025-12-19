using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HexMaster.Attendr.Core.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrCache(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<AttendrCacheOptions>()
            .BindConfiguration(AttendrCacheOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "ConnectionString is required");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AttendrCacheOptions>>().Value;
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        });

        services.AddSingleton<IAttendrCacheClient>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            var opts = sp.GetRequiredService<IOptions<AttendrCacheOptions>>().Value;
            return new AttendrCacheClient(multiplexer, opts);
        });

        return services;
    }
}
