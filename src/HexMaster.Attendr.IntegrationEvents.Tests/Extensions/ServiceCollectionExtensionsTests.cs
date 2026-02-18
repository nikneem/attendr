using HexMaster.Attendr.IntegrationEvents.Extensions;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder().Build();

    [Fact]
    public void AddIntegrationEvents_RegistersIIntegrationEventPublisher()
    {
        var services = new ServiceCollection();
        services.AddIntegrationEvents(CreateConfiguration());

        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IIntegrationEventPublisher));

        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddIntegrationEvents_RegistersAsScoped()
    {
        var services = new ServiceCollection();
        services.AddIntegrationEvents(CreateConfiguration());

        var descriptor = services.First(s => s.ServiceType == typeof(IIntegrationEventPublisher));

        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddIntegrationEvents_RegistersIntegrationEventPublisherImplementation()
    {
        var services = new ServiceCollection();
        services.AddIntegrationEvents(CreateConfiguration());

        var descriptor = services.First(s => s.ServiceType == typeof(IIntegrationEventPublisher));

        Assert.Equal(typeof(IntegrationEventPublisher), descriptor.ImplementationType);
    }

    [Fact]
    public void AddIntegrationEvents_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddIntegrationEvents(CreateConfiguration());

        Assert.Same(services, result);
    }
}
