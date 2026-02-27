using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Tests.DomainModels;

public sealed class PresentationSpeakerTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateSpeaker()
    {
        var id = Guid.NewGuid();

        var speaker = new PresentationSpeaker(id, "Alice Smith", "https://example.com/photo.jpg");

        Assert.Equal(id, speaker.SpeakerId);
        Assert.Equal("Alice Smith", speaker.Name);
        Assert.Equal("https://example.com/photo.jpg", speaker.ProfilePictureUrl);
    }

    [Fact]
    public void Constructor_WithNullProfilePictureUrl_ShouldCreateSpeaker()
    {
        var speaker = new PresentationSpeaker(Guid.NewGuid(), "Alice Smith", null);

        Assert.Null(speaker.ProfilePictureUrl);
    }

    [Fact]
    public void Constructor_WithEmptySpeakerId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationSpeaker(Guid.Empty, "Alice Smith", null));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationSpeaker(Guid.NewGuid(), "   ", null));
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PresentationSpeaker(Guid.NewGuid(), null!, null));
    }
}
