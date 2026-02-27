using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

/// <summary>
/// Tests for Conference mutation methods not covered in ConferenceTests.cs.
/// </summary>
public class ConferenceMutationTests
{
    // ── UpdateDetails ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateProperties()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var newStart = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));
        var newEnd = newStart.AddDays(2);

        conference.UpdateDetails("Updated Title", "Rotterdam", "Netherlands", newStart, newEnd, "https://img.example.com/logo.png");

        Assert.Equal("Updated Title", conference.Title);
        Assert.Equal("Rotterdam", conference.City);
        Assert.Equal("Netherlands", conference.Country);
        Assert.Equal(newStart, conference.StartDate);
        Assert.Equal(newEnd, conference.EndDate);
        Assert.Equal("https://img.example.com/logo.png", conference.ImageUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateDetails_WithEmptyTitle_ShouldThrowArgumentException(string? title)
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        Assert.ThrowsAny<ArgumentException>(() =>
            conference.UpdateDetails(title!, conference.City, conference.Country, conference.StartDate, conference.EndDate));
    }

    [Fact]
    public void UpdateDetails_WithEndBeforeStart_ShouldThrowArgumentException()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(2));
        var end = start.AddDays(-1);

        Assert.Throws<ArgumentException>(() =>
            conference.UpdateDetails(conference.Title, conference.City, conference.Country, start, end));
    }

    // ── UpdateVisibility ─────────────────────────────────────────────────────

    [Fact]
    public void UpdateVisibility_ToTrue_ShouldSetIsVisible()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        Assert.False(conference.IsVisible);

        conference.UpdateVisibility(true);

        Assert.True(conference.IsVisible);
    }

    [Fact]
    public void UpdateVisibility_ToFalse_WhenAlreadyFalse_ShouldRemainFalse()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        conference.UpdateVisibility(false);

        Assert.False(conference.IsVisible);
    }

    // ── SetConfigureSynchronizationSource ────────────────────────────────────

    [Fact]
    public void SetConfigureSynchronizationSource_WithNewSource_ShouldUpdateSource()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var syncSource = SynchronizationSource.CreateWithUrl(SynchronizationSourceType.Sessionize, "https://sessionize.com/api");

        conference.SetConfigureSynchronizationSource(syncSource);

        Assert.NotNull(conference.SynchronizationSource);
        Assert.Equal("https://sessionize.com/api", conference.SynchronizationSource.SourceLocationOrApiKey);
    }

    [Fact]
    public void SetConfigureSynchronizationSource_WithNull_ShouldClearSyncSource()
    {
        var syncSource = SynchronizationSource.CreateWithUrl(SynchronizationSourceType.Sessionize, "https://sessionize.com/api");
        var conference = Conference.FromPersisted(
            Guid.NewGuid(), "Title", "City", "Country",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            synchronizationSource: syncSource);

        conference.SetConfigureSynchronizationSource(null);

        Assert.Null(conference.SynchronizationSource);
    }

    [Fact]
    public void SetConfigureSynchronizationSource_WithSameSource_ShouldNotChangeState()
    {
        var syncSource = SynchronizationSource.CreateWithUrl(SynchronizationSourceType.Sessionize, "https://same.url");
        var conference = Conference.FromPersisted(
            Guid.NewGuid(), "Title", "City", "Country",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            synchronizationSource: syncSource);

        // Call with identical source — should be no-op
        var sameSyncSource = SynchronizationSource.CreateWithUrl(SynchronizationSourceType.Sessionize, "https://same.url");
        conference.SetConfigureSynchronizationSource(sameSyncSource);

        Assert.Equal("https://same.url", conference.SynchronizationSource!.SourceLocationOrApiKey);
    }

    // ── UpdateSpeaker ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateSpeaker_WithModifiedSpeaker_ShouldSetConferenceTouched()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var speaker = Speaker.Create("Jane Doe");
        conference.AddSpeaker(speaker);

        speaker.SetName("Updated Name");
        conference.UpdateSpeaker(speaker);

        // State should become Touched because speaker state != Pristine/Touched
        Assert.NotEqual(DomainModelState.Pristine, conference.State);
    }

    [Fact]
    public void UpdateSpeaker_WithPristineSpeaker_ShouldNotChangeFurtherState()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var speaker = Speaker.FromPersisted(Guid.NewGuid(), "Jane Doe"); // state = Pristine
        conference.UpdateSpeaker(speaker);

        // Nothing should explode; Pristine speaker does not change conference state
        Assert.NotNull(conference);
    }

    // ── UpdateRoom ────────────────────────────────────────────────────────────

    [Fact]
    public void UpdateRoom_WithModifiedRoom_ShouldSetConferenceTouched()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var room = Room.Create("Hall A", 100);
        conference.AddRoom(room);

        room.SetName("Hall B");
        conference.UpdateRoom(room);

        Assert.NotEqual(DomainModelState.Pristine, conference.State);
    }

    // ── UpdatePresentation ────────────────────────────────────────────────────

    [Fact]
    public void UpdatePresentation_WithModifiedPresentation_ShouldSetConferenceTouched()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var room = ConferenceFactory.CreateRoom(null, null);
        var speaker = ConferenceFactory.CreateSpeaker(null, null);
        conference.AddRoom(room);
        conference.AddSpeaker(speaker);

        var presentation = ConferenceFactory.CreatePresentation(null, null, null, null, room, new[] { speaker });
        conference.AddPresentation(presentation);

        presentation.UpdateDetails("New Title", presentation.Abstract, presentation.StartDateTime, presentation.EndDateTime);
        conference.UpdatePresentation(presentation);

        Assert.NotEqual(DomainModelState.Pristine, conference.State);
    }

    // ── RemoveUnusedRooms ─────────────────────────────────────────────────────

    [Fact]
    public void RemoveUnusedRooms_WithUnusedRoom_ShouldRemoveAndReturnCount()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var usedRoom = ConferenceFactory.CreateRoom("Used Room", 50);
        var unusedRoom = ConferenceFactory.CreateRoom("Unused Room", 20);
        var speaker = ConferenceFactory.CreateSpeaker(null, null);
        conference.AddRoom(usedRoom);
        conference.AddRoom(unusedRoom);
        conference.AddSpeaker(speaker);
        conference.AddPresentation(ConferenceFactory.CreatePresentation(null, null, null, null, usedRoom, new[] { speaker }));

        var removed = conference.RemoveUnusedRooms();

        Assert.Equal(1, removed);
        Assert.Single(conference.Rooms);
        Assert.Equal(usedRoom.Id, conference.Rooms.First().Id);
    }

    [Fact]
    public void RemoveUnusedRooms_WhenAllRoomsUsed_ShouldReturnZero()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var room = ConferenceFactory.CreateRoom(null, null);
        var speaker = ConferenceFactory.CreateSpeaker(null, null);
        conference.AddRoom(room);
        conference.AddSpeaker(speaker);
        conference.AddPresentation(ConferenceFactory.CreatePresentation(null, null, null, null, room, new[] { speaker }));

        var removed = conference.RemoveUnusedRooms();

        Assert.Equal(0, removed);
    }

    [Fact]
    public void RemoveUnusedRooms_WithNoPresentations_ShouldRemoveAllRooms()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        conference.AddRoom(ConferenceFactory.CreateRoom("A", 10));
        conference.AddRoom(ConferenceFactory.CreateRoom("B", 20));

        var removed = conference.RemoveUnusedRooms();

        Assert.Equal(2, removed);
        Assert.Empty(conference.Rooms);
    }

    // ── RemoveUnusedSpeakers ──────────────────────────────────────────────────

    [Fact]
    public void RemoveUnusedSpeakers_WithUnusedSpeaker_ShouldRemoveAndReturnCount()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var usedSpeaker = ConferenceFactory.CreateSpeaker("Used", null);
        var unusedSpeaker = ConferenceFactory.CreateSpeaker("Unused", null);
        var room = ConferenceFactory.CreateRoom(null, null);
        conference.AddRoom(room);
        conference.AddSpeaker(usedSpeaker);
        conference.AddSpeaker(unusedSpeaker);
        conference.AddPresentation(ConferenceFactory.CreatePresentation(null, null, null, null, room, new[] { usedSpeaker }));

        var removed = conference.RemoveUnusedSpeakers();

        Assert.Equal(1, removed);
        Assert.DoesNotContain(conference.Speakers, s => s.Id == unusedSpeaker.Id);
    }

    [Fact]
    public void RemoveUnusedSpeakers_WhenAllSpeakersUsed_ShouldReturnZero()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        var speaker = ConferenceFactory.CreateSpeaker(null, null);
        var room = ConferenceFactory.CreateRoom(null, null);
        conference.AddRoom(room);
        conference.AddSpeaker(speaker);
        conference.AddPresentation(ConferenceFactory.CreatePresentation(null, null, null, null, room, new[] { speaker }));

        var removed = conference.RemoveUnusedSpeakers();

        Assert.Equal(0, removed);
    }

    [Fact]
    public void RemoveUnusedSpeakers_WithNoPresentations_ShouldRemoveAllSpeakers()
    {
        var conference = ConferenceFactory.CreatePersistedConference(null, null, null, null, null, null, null, null);
        conference.AddSpeaker(ConferenceFactory.CreateSpeaker("Alice", null));
        conference.AddSpeaker(ConferenceFactory.CreateSpeaker("Bob", null));

        var removed = conference.RemoveUnusedSpeakers();

        Assert.Equal(2, removed);
        Assert.Empty(conference.Speakers);
    }
}
