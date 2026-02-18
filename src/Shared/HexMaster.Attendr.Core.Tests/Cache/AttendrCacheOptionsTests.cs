using HexMaster.Attendr.Core.Cache;

namespace HexMaster.Attendr.Core.Tests.Cache;

public sealed class AttendrCacheOptionsTests
{
    [Fact]
    public void SectionName_IsCorrect()
    {
        Assert.Equal("Attendr:Cache", AttendrCacheOptions.SectionName);
    }

    [Fact]
    public void DefaultStoreName_IsEmpty()
    {
        var options = new AttendrCacheOptions();
        Assert.Equal(string.Empty, options.StoreName);
    }

    [Fact]
    public void DefaultTtlSeconds_Is300()
    {
        var options = new AttendrCacheOptions();
        Assert.Equal(300, options.DefaultTtlSeconds);
    }

    [Fact]
    public void StoreName_CanBeSet()
    {
        var options = new AttendrCacheOptions { StoreName = "my-store" };
        Assert.Equal("my-store", options.StoreName);
    }

    [Fact]
    public void DefaultTtlSeconds_CanBeSet()
    {
        var options = new AttendrCacheOptions { DefaultTtlSeconds = 600 };
        Assert.Equal(600, options.DefaultTtlSeconds);
    }
}
