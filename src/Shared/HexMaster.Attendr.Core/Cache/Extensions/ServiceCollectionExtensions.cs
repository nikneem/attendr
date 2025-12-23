using Dapr.Client;
using HexMaster.Attendr.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Core.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DaprOptions>(configuration.GetSection(DaprOptions.SectionName));
        services.AddScoped<IAttendrCacheClient, AttendrCacheClient>();

        return services;
    }
}
