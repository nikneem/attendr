using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Tests.Factories;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

/// <summary>
/// Tests for Conference.FromPersisted constructor validation and AddSpeaker/AddPresentation duplicate logic.
/// </summary>
public class ConferenceFromPersistedValidationTests
{
    private static readonly DateOnly DefaultStart = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));
    private static readonly DateOnly DefaultEnd = DefaultStart.AddDays(3);

    [Fact]
    public void FromPersisted_WithEmptyGuid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Conference.FromPersisted(Guid.Empty, "Title", "City", "Country", DefaultStart, DefaultEnd));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromPersisted_WithEmptyTitle_ThrowsArgumentException(string title)
    {
        Assert.Throws<ArgumentException>(() =>
            Conference.FromPersisted(Guid.NewGuid(), title, "City", "Country", DefaultStart, DefaultEnd));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromPersisted_WithEmptyCity_ThrowsArgumentException(string city)
    {
        Assert.Throws<ArgumentException>(() =>
            Conference.FromPersisted(Guid.NewGuid(), "Title", city, "Country", DefaultStart, DefaultEnd));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FromPersisted_WithEmptyCountry_ThrowsArgumentException(string country)
    {
        Assert.Throws<ArgumentException>(() =>
            Conference.FromPersisted(Guid.NewGuid(), "Title", "City", country, DefaultStart, DefaultEnd));
    }

    [Fact]
    public void FromPersisted_WithEndBeforeStart_ThrowsArgumentException()
    {
        var end = DefaultStart.AddDays(-1);
        Assert.Throws<ArgumentException>(() =>
            Conference.FromPersisted(Guid.NewGuid(), "Title", "City", "Country", DefaultStart, end));
    }

    [Fact]
    public void AddRoom_DuplicateId_ThrowsInvalidOperationException()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        var room = ConferenceFactory.CreateRoom();
        conference.AddRoom(room);

        Assert.Throws<InvalidOperationException>(() => conference.AddRoom(room));
    }

    [Fact]
    public void AddSpeaker_DuplicateId_ThrowsInvalidOperationException()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        var speaker = ConferenceFactory.CreateSpeaker();
        conference.AddSpeaker(speaker);

        Assert.Throws<InvalidOperationException>(() => conference.AddSpeaker(speaker));
    }

    [Fact]
    public void AddPresentation_DuplicateId_ThrowsInvalidOperationException()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        var room = ConferenceFactory.CreateRoom();
        var speaker = ConferenceFactory.CreateSpeaker();
        conference.AddRoom(room);
        conference.AddSpeaker(speaker);
        var presentation = ConferenceFactory.CreatePresentation(null, null, null, null, room, new[] { speaker });
        conference.AddPresentation(presentation);

        Assert.Throws<InvalidOperationException>(() => conference.AddPresentation(presentation));
    }
}
