using HexMaster.Attendr.Core.Configuration;

namespace HexMaster.Attendr.Core.Tests.Configuration;

public sealed class AttendrConfigurationTests
{
    [Fact]
    public void SectionName_IsAttendر()
    {
        Assert.Equal("Attendr", AttendrConfiguration.SectionName);
    }

    [Fact]
    public void DefaultConstructor_CreatesIntegrationEndpointsInstance()
    {
        var config = new AttendrConfiguration();
        Assert.NotNull(config.Integration);
    }

    [Fact]
    public void Integration_CanSetProfiles()
    {
        var config = new AttendrConfiguration();
        config.Integration.Profiles = "https://profiles.example.com";
        Assert.Equal("https://profiles.example.com", config.Integration.Profiles);
    }

    [Fact]
    public void Integration_CanSetGroups()
    {
        var config = new AttendrConfiguration();
        config.Integration.Groups = "https://groups.example.com";
        Assert.Equal("https://groups.example.com", config.Integration.Groups);
    }

    [Fact]
    public void Integration_CanSetConferences()
    {
        var config = new AttendrConfiguration();
        config.Integration.Conferences = "https://conferences.example.com";
        Assert.Equal("https://conferences.example.com", config.Integration.Conferences);
    }

    [Fact]
    public void IntegrationEndpoints_DefaultValues_AreEmpty()
    {
        var endpoints = new IntegrationEndpoints();
        Assert.Equal(string.Empty, endpoints.Profiles);
        Assert.Equal(string.Empty, endpoints.Groups);
        Assert.Equal(string.Empty, endpoints.Conferences);
    }
}
