using System.Diagnostics;
using HexMaster.Attendr.Core.Observability;

namespace HexMaster.Attendr.Core.Tests.Observability;

public sealed class ActivitySourcesTests
{
    [Fact]
    public void Profiles_ActivitySource_IsNotNull()
    {
        Assert.NotNull(ActivitySources.Profiles);
    }

    [Fact]
    public void Profiles_ActivitySource_HasCorrectName()
    {
        Assert.Equal("HexMaster.Attendr.Profiles", ActivitySources.Profiles.Name);
    }

    [Fact]
    public void Groups_ActivitySource_IsNotNull()
    {
        Assert.NotNull(ActivitySources.Groups);
    }

    [Fact]
    public void Groups_ActivitySource_HasCorrectName()
    {
        Assert.Equal("HexMaster.Attendr.Groups", ActivitySources.Groups.Name);
    }

    [Fact]
    public void Conferences_ActivitySource_IsNotNull()
    {
        Assert.NotNull(ActivitySources.Conferences);
    }

    [Fact]
    public void Conferences_ActivitySource_HasCorrectName()
    {
        Assert.Equal("HexMaster.Attendr.Conferences", ActivitySources.Conferences.Name);
    }

    [Fact]
    public void Presence_ActivitySource_IsNotNull()
    {
        Assert.NotNull(ActivitySources.Presence);
    }

    [Fact]
    public void Presence_ActivitySource_HasCorrectName()
    {
        Assert.Equal("HexMaster.Attendr.Presence", ActivitySources.Presence.Name);
    }

    [Fact]
    public void Proxy_ActivitySource_IsNotNull()
    {
        Assert.NotNull(ActivitySources.Proxy);
    }

    [Fact]
    public void Proxy_ActivitySource_HasCorrectName()
    {
        Assert.Equal("HexMaster.Attendr.Proxy", ActivitySources.Proxy.Name);
    }

    [Fact]
    public void AllActivitySources_AreActivitySourceInstances()
    {
        Assert.IsType<ActivitySource>(ActivitySources.Profiles);
        Assert.IsType<ActivitySource>(ActivitySources.Groups);
        Assert.IsType<ActivitySource>(ActivitySources.Conferences);
        Assert.IsType<ActivitySource>(ActivitySources.Presence);
        Assert.IsType<ActivitySource>(ActivitySources.Proxy);
    }
}
