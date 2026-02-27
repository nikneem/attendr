using HexMaster.Attendr.Conferences.Extensions;
using HexMaster.Attendr.Core.CommandHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Conferences.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAttendrConferencesServices_RegistersAllHandlers()
    {
        var services = new ServiceCollection();

        var result = services.AddAttendrConferencesServices();

        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.NotEmpty(services);
    }

    [Fact]
    public void AddAttendrConferencesServices_IsChainable()
    {
        var services = new ServiceCollection();

        var chainResult = services
            .AddAttendrConferencesServices()
            .AddAttendrConferencesServices(); // calling twice should not throw

        Assert.NotNull(chainResult);
    }

    [Fact]
    public void AddSemanticKernelForConferences_MissingEndpoint_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build(); // Empty config

        Assert.Throws<InvalidOperationException>(() =>
            services.AddSemanticKernelForConferences(configuration));
    }

    [Fact]
    public void AddSemanticKernelForConferences_PartialConfig_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string?>
        {
            ["AzureOpenAI:Endpoint"] = "https://example.openai.azure.com/",
            // DeploymentName and ApiKey are missing
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        Assert.Throws<InvalidOperationException>(() =>
            services.AddSemanticKernelForConferences(configuration));
    }

    [Fact]
    public void AddSemanticKernelForConferences_ValidConfig_RegistersServices()
    {
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string?>
        {
            ["AzureOpenAI:Endpoint"] = "https://myopenai.openai.azure.com/",
            ["AzureOpenAI:DeploymentName"] = "gpt-4",
            ["AzureOpenAI:ApiKey"] = "fake-api-key-for-testing"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var result = services.AddSemanticKernelForConferences(configuration);

        Assert.NotNull(result);
        Assert.Same(services, result);
    }
}
