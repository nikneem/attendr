using HexMaster.Attendr.Core.Cache;

namespace HexMaster.Attendr.Core.Tests.Cache;

public sealed class CacheKeysTests
{
    // ──────────────────────────── Conferences ────────────────────────────

    [Fact]
    public void Conferences_Metrics_IsCorrectKey()
    {
        Assert.Equal("conferences:metrics", CacheKeys.Conferences.Metrics);
    }

    // ──────────────────────────── Profiles ────────────────────────────

    [Fact]
    public void Profiles_Metrics_IsCorrectKey()
    {
        Assert.Equal("profiles:metrics", CacheKeys.Profiles.Metrics);
    }

    [Fact]
    public void Profiles_Subject_WithValidId_ReturnsExpectedKey()
    {
        var result = CacheKeys.Profiles.Subject("abc123");
        Assert.Equal("profiles:subject:abc123", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Profiles_Subject_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        Assert.Throws<ArgumentException>(() => CacheKeys.Profiles.Subject(invalidId!));
    }

    [Fact]
    public void Profiles_Details_WithValidId_ReturnsExpectedKey()
    {
        var profileId = Guid.NewGuid().ToString();
        var result = CacheKeys.Profiles.Details(profileId);
        Assert.Equal($"profiles:details:{profileId}", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Profiles_Details_WithInvalidId_ThrowsArgumentException(string? invalidId)
    {
        Assert.Throws<ArgumentException>(() => CacheKeys.Profiles.Details(invalidId!));
    }
}
