using Dapr.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Core.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAttendrCacheClient, AttendrCacheClient>();

        return services;
    }
}
