using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

public class CheckInTests
{
    private static PresentationData CreatePresentationData(Guid? id = null)
    {
        return new PresentationData(
            id ?? Guid.NewGuid(),
            "Test Title",
            "Test abstract content",
            "Room B",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            Array.Empty<PresentationSpeaker>());
    }

    // --- CheckIn ---

    [Fact]
    public void Create_WithValidArgs_ShouldReturnCheckIn()
    {
        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(2));
        Assert.NotEqual(Guid.Empty, checkIn.Id);
        Assert.Empty(checkIn.Members);
    }

    [Fact]
    public void Create_WithEmptyGroupId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CheckIn.Create(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_WithEmptyConferenceId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CheckIn.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_WithEmptyPresentationId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, CreatePresentationData(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_WithNullPresentationData_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null!, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void FromPersisted_WithMembers_ShouldHaveMembers()
    {
        var memberId = Guid.NewGuid();
        var member = new CheckedInMember(memberId, "Member Name", null);
        var checkIn = CheckIn.FromPersisted(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(2),
            new[] { member });
        Assert.Single(checkIn.Members);
        Assert.Equal(memberId, checkIn.Members.First().Id);
    }

    [Fact]
    public void FromPersisted_WithEmptyId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            CheckIn.FromPersisted(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                CreatePresentationData(), DateTimeOffset.UtcNow, null));
    }

    [Fact]
    public void AddMember_WithValidMember_ShouldAddSuccessfully()
    {
        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(2));
        var member = new CheckedInMember(Guid.NewGuid(), "John Doe", null);
        checkIn.AddMember(member);
        Assert.Single(checkIn.Members);
    }

    [Fact]
    public void AddMember_WithNull_ShouldThrowArgumentNullException()
    {
        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(2));
        Assert.Throws<ArgumentNullException>(() => checkIn.AddMember(null!));
    }

    [Fact]
    public void AddMember_DuplicateMember_ShouldThrowInvalidOperationException()
    {
        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(2));
        var memberId = Guid.NewGuid();
        var member = new CheckedInMember(memberId, "Jane", null);
        checkIn.AddMember(member);
        Assert.Throws<InvalidOperationException>(() => checkIn.AddMember(new CheckedInMember(memberId, "Jane Duplicate", null)));
    }

    [Fact]
    public void RemoveMember_ExistingMember_ShouldRemoveSuccessfully()
    {
        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(2));
        var memberId = Guid.NewGuid();
        checkIn.AddMember(new CheckedInMember(memberId, "Remove Me", null));
        checkIn.RemoveMember(memberId);
        Assert.Empty(checkIn.Members);
    }

    [Fact]
    public void RemoveMember_NonExistentMember_ShouldDoNothing()
    {
        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(2));
        // Should not throw
        checkIn.RemoveMember(Guid.NewGuid());
        Assert.Empty(checkIn.Members);
    }

    [Fact]
    public void IsExpired_WhenPastExpiration_ShouldReturnTrue()
    {
        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(-1));
        Assert.True(checkIn.IsExpired(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsExpired_WhenFutureExpiration_ShouldReturnFalse()
    {
        var checkIn = CheckIn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CreatePresentationData(), DateTimeOffset.UtcNow.AddHours(5));
        Assert.False(checkIn.IsExpired(DateTimeOffset.UtcNow));
    }

    // --- PresentationData ---

    [Fact]
    public void PresentationData_Constructor_WithValidArgs_ShouldCreate()
    {
        var data = new PresentationData(Guid.NewGuid(), "Title", "Abstract", "Room", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), Array.Empty<PresentationSpeaker>());
        Assert.Equal("Title", data.Title);
        Assert.Equal("Abstract", data.Abstract);
        Assert.Equal("Room", data.Room);
    }

    [Fact]
    public void PresentationData_Constructor_WithEmptyId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationData(Guid.Empty, "Title", "Abstract", "Room", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), Array.Empty<PresentationSpeaker>()));
    }

    [Fact]
    public void PresentationData_Constructor_WithNullTitle_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PresentationData(Guid.NewGuid(), null!, "Abstract", "Room", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), Array.Empty<PresentationSpeaker>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PresentationData_Constructor_WithEmptyOrWhitespaceTitle_ShouldThrowArgumentException(string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationData(Guid.NewGuid(), title, "Abstract", "Room", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), Array.Empty<PresentationSpeaker>()));
    }

    [Fact]
    public void PresentationData_Constructor_WithNullAbstract_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PresentationData(Guid.NewGuid(), "Title", null!, "Room", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), Array.Empty<PresentationSpeaker>()));
    }

    [Fact]
    public void PresentationData_Constructor_WithNullRoom_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PresentationData(Guid.NewGuid(), "Title", "Abstract", null!, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), Array.Empty<PresentationSpeaker>()));
    }

    [Fact]
    public void PresentationData_Constructor_WithNullSpeakers_ShouldHaveEmptySpeakers()
    {
        var data = new PresentationData(Guid.NewGuid(), "Title", "Abstract", "Room", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), null!);
        Assert.Empty(data.Speakers);
    }

    // --- PresentationSpeaker ---

    [Fact]
    public void PresentationSpeaker_Constructor_WithValidArgs_ShouldCreate()
    {
        var id = Guid.NewGuid();
        var speaker = new PresentationSpeaker(id, "Speaker Name", "https://example.com/pic.jpg");
        Assert.Equal(id, speaker.Id);
        Assert.Equal("Speaker Name", speaker.Name);
        Assert.Equal("https://example.com/pic.jpg", speaker.ProfilePictureUrl);
    }

    [Fact]
    public void PresentationSpeaker_Constructor_WithEmptyId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new PresentationSpeaker(Guid.Empty, "Name", null));
    }

    [Fact]
    public void PresentationSpeaker_Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PresentationSpeaker(Guid.NewGuid(), null!, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void PresentationSpeaker_Constructor_WithEmptyName_ShouldThrowArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => new PresentationSpeaker(Guid.NewGuid(), name, null));
    }

    [Fact]
    public void PresentationSpeaker_Constructor_WithNullProfilePicture_ShouldCreate()
    {
        var speaker = new PresentationSpeaker(Guid.NewGuid(), "Speaker", null);
        Assert.Null(speaker.ProfilePictureUrl);
    }

    // --- CheckedInMember ---

    [Fact]
    public void CheckedInMember_Constructor_WithValidArgs_ShouldCreate()
    {
        var id = Guid.NewGuid();
        var member = new CheckedInMember(id, "Member Name", "https://example.com/pic.jpg");
        Assert.Equal(id, member.Id);
        Assert.Equal("Member Name", member.Name);
        Assert.Equal("https://example.com/pic.jpg", member.ProfilePictureUrl);
    }

    [Fact]
    public void CheckedInMember_Constructor_WithEmptyId_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new CheckedInMember(Guid.Empty, "Name", null));
    }

    [Fact]
    public void CheckedInMember_Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CheckedInMember(Guid.NewGuid(), null!, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void CheckedInMember_Constructor_WithEmptyName_ShouldThrowArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => new CheckedInMember(Guid.NewGuid(), name, null));
    }

    [Fact]
    public void CheckedInMember_Constructor_WithNullProfilePicture_ShouldCreate()
    {
        var member = new CheckedInMember(Guid.NewGuid(), "Member", null);
        Assert.Null(member.ProfilePictureUrl);
    }
}
