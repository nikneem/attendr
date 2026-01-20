using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.Extensions;
using HexMaster.Attendr.Notifications.Features.ProcessNotificationTrigger;
using HexMaster.Attendr.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Notifications.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNotificationFeatures_RegistersNotificationTypeService()
    {
        var services = new ServiceCollection();

        services.AddNotificationFeatures();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(INotificationTypeService));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor!.Lifetime);
        Assert.Equal(typeof(NotificationTypeService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddNotificationFeatures_RegistersNotificationService()
    {
        var services = new ServiceCollection();

        services.AddNotificationFeatures();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(INotificationService));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
        Assert.Equal(typeof(NotificationService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddNotificationFeatures_RegistersPushNotificationService()
    {
        var services = new ServiceCollection();

        services.AddNotificationFeatures();

        // HttpClient services are registered differently, check if interface is registered
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPushNotificationService));

        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddNotificationFeatures_RegistersNotificationPreferencesCacheService()
    {
        var services = new ServiceCollection();

        services.AddNotificationFeatures();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(INotificationPreferencesCacheService));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
        Assert.Equal(typeof(NotificationPreferencesCacheService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddNotificationFeatures_RegistersEmailNotificationService()
    {
        var services = new ServiceCollection();

        services.AddNotificationFeatures();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmailNotificationService));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
        Assert.Equal(typeof(EmailNotificationService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddNotificationFeatures_RegistersCommandHandler()
    {
        var services = new ServiceCollection();

        services.AddNotificationFeatures();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<ProcessNotificationTriggerCommand>));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
        Assert.Equal(typeof(ProcessNotificationTriggerCommandHandler), descriptor.ImplementationType);
    }

    [Fact]
    public void AddNotificationFeatures_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddNotificationFeatures();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddNotificationFeatures_RegistersMemoryCache()
    {
        var services = new ServiceCollection();

        services.AddNotificationFeatures();

        // Verify memory cache is registered (needed for preferences caching)
        var memoryCacheDescriptor = services.FirstOrDefault(d => d.ServiceType.Name.Contains("IMemoryCache"));

        Assert.NotNull(memoryCacheDescriptor);
    }
}
