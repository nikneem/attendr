using Dapr.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.Core.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrCache(this IServiceCollection services)
    {
        services.AddScoped<IAttendrCacheClient, AttendrCacheClient>();

        return services;
    }
}
