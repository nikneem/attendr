using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

/// <summary>
/// Tests for Speaker mutation methods not covered in SpeakerTests.cs.
/// </summary>
public class SpeakerMutationTests
{
    // ── SetName ─────────────────────────────────────────────────────────────

    [Fact]
    public void SetName_WithNewName_ShouldUpdateName()
    {
        var speaker = Speaker.Create("Old Name");

        speaker.SetName("New Name");

        Assert.Equal("New Name", speaker.Name);
    }

    [Fact]
    public void SetName_WithSameName_ShouldNotChangeName()
    {
        var speaker = Speaker.Create("Same Name");

        speaker.SetName("Same Name");

        Assert.Equal("Same Name", speaker.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetName_WithEmptyOrNull_ShouldThrowArgumentException(string? name)
    {
        var speaker = Speaker.Create("Alice");
        Assert.ThrowsAny<ArgumentException>(() => speaker.SetName(name!));
    }

    // ── SetCompany ───────────────────────────────────────────────────────────

    [Fact]
    public void SetCompany_WithCompanyName_ShouldUpdateCompany()
    {
        var speaker = Speaker.Create("Alice");

        speaker.SetCompany("Contoso");

        Assert.Equal("Contoso", speaker.Company);
    }

    [Fact]
    public void SetCompany_WithNull_ShouldClearCompany()
    {
        var speaker = Speaker.Create("Alice", company: "Old Corp");

        speaker.SetCompany(null);

        Assert.Null(speaker.Company);
    }

    [Fact]
    public void SetCompany_WithSameValue_ShouldNotChange()
    {
        var speaker = Speaker.Create("Alice", company: "Contoso");

        speaker.SetCompany("Contoso");

        Assert.Equal("Contoso", speaker.Company);
    }

    // ── SetProfilePictureUrl ─────────────────────────────────────────────────

    [Fact]
    public void SetProfilePictureUrl_WithUrl_ShouldUpdateUrl()
    {
        var speaker = Speaker.Create("Alice");

        speaker.SetProfilePictureUrl("https://example.com/pic.jpg");

        Assert.Equal("https://example.com/pic.jpg", speaker.ProfilePictureUrl);
    }

    [Fact]
    public void SetProfilePictureUrl_WithNull_ShouldClearUrl()
    {
        var speaker = Speaker.Create("Alice", profilePictureUrl: "https://old.jpg");

        speaker.SetProfilePictureUrl(null);

        Assert.Null(speaker.ProfilePictureUrl);
    }

    [Fact]
    public void SetProfilePictureUrl_WithSameUrl_ShouldNotChange()
    {
        const string url = "https://same.jpg";
        var speaker = Speaker.Create("Alice", profilePictureUrl: url);

        speaker.SetProfilePictureUrl(url);

        Assert.Equal(url, speaker.ProfilePictureUrl);
    }
}
