using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Core.Cache.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Core.Tests.Cache;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAttendrCache_RegistersIAttendrCacheClientAsScoped()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddAttendrCache(configuration);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAttendrCacheClient));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
        Assert.Equal(typeof(AttendrCacheClient), descriptor.ImplementationType);
    }

    [Fact]
    public void AddAttendrCache_ReturnsServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var result = services.AddAttendrCache(configuration);

        Assert.Same(services, result);
    }
}
