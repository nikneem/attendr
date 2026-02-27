using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

public class SynchronizationSourceTests
{
    // ── CreateWithUrl ───────────────────────────────────────────────────────

    [Fact]
    public void CreateWithUrl_WithValidData_ShouldReturnInstance()
    {
        var source = SynchronizationSource.CreateWithUrl(
            SynchronizationSourceType.Sessionize,
            "https://sessionize.com/api/v2/test123");

        Assert.NotNull(source);
        Assert.Equal(SynchronizationSourceType.Sessionize, source.SourceType);
        Assert.Equal("https://sessionize.com/api/v2/test123", source.SourceLocationOrApiKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateWithUrl_WithEmptyLocation_ShouldThrowArgumentException(string? location)
    {
        Assert.Throws<ArgumentException>(() =>
            SynchronizationSource.CreateWithUrl(SynchronizationSourceType.Sessionize, location!));
    }

    // ── FromPersisted ───────────────────────────────────────────────────────

    [Fact]
    public void FromPersisted_WithNullLocation_ShouldSucceed()
    {
        var source = SynchronizationSource.FromPersisted(SynchronizationSourceType.Sessionize, null);

        Assert.NotNull(source);
        Assert.Equal(SynchronizationSourceType.Sessionize, source.SourceType);
        Assert.Null(source.SourceLocationOrApiKey);
    }

    [Fact]
    public void FromPersisted_WithLocation_ShouldRestoreProperties()
    {
        const string url = "https://sessionize.com/api/v2/abc";
        var source = SynchronizationSource.FromPersisted(SynchronizationSourceType.Sessionize, url);

        Assert.Equal(url, source.SourceLocationOrApiKey);
    }

    // ── UpdateSourceLocation ────────────────────────────────────────────────

    [Fact]
    public void UpdateSourceLocation_WithValidUrl_ShouldUpdateProperty()
    {
        var source = SynchronizationSource.CreateWithUrl(
            SynchronizationSourceType.Sessionize, "https://old.url");

        source.UpdateSourceLocation("https://new.url");

        Assert.Equal("https://new.url", source.SourceLocationOrApiKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateSourceLocation_WithEmptyUrl_ShouldThrowArgumentException(string? url)
    {
        var source = SynchronizationSource.CreateWithUrl(
            SynchronizationSourceType.Sessionize, "https://existing.url");

        Assert.Throws<ArgumentException>(() => source.UpdateSourceLocation(url!));
    }
}
