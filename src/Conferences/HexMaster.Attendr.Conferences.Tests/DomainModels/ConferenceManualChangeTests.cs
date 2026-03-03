using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

public sealed class ConferenceManualChangeTests
{
    [Fact]
    public void MarkInvisibleDueToManualChanges_WhenVisible_ShouldSetInvisible()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        conference.UpdateVisibility(true);
        Assert.True(conference.IsVisible);

        conference.MarkInvisibleDueToManualChanges();

        Assert.False(conference.IsVisible);
        Assert.Equal(DomainModelState.Modified, conference.State);
    }

    [Fact]
    public void MarkInvisibleDueToManualChanges_WhenAlreadyInvisible_ShouldNotChangeState()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        Assert.False(conference.IsVisible);

        conference.MarkInvisibleDueToManualChanges();

        Assert.False(conference.IsVisible);
    }

    [Fact]
    public void RemoveSpeaker_ExistingSpeaker_ShouldRemove()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        var speaker = ConferenceFactory.CreateSpeaker();
        conference.AddSpeaker(speaker);
        Assert.Single(conference.Speakers);

        conference.RemoveSpeaker(speaker.Id);

        Assert.Empty(conference.Speakers);
    }

    [Fact]
    public void RemoveSpeaker_NonExistentSpeaker_ShouldThrow()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        Assert.Throws<InvalidOperationException>(() => conference.RemoveSpeaker(Guid.NewGuid()));
    }

    [Fact]
    public void RemoveRoom_ExistingRoom_ShouldRemove()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        var room = ConferenceFactory.CreateRoom();
        conference.AddRoom(room);

        conference.RemoveRoom(room.Id);

        Assert.Empty(conference.Rooms);
    }

    [Fact]
    public void RemoveRoom_UsedByPresentation_ShouldThrow()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        var room = ConferenceFactory.CreateRoom();
        var speaker = ConferenceFactory.CreateSpeaker();
        conference.AddRoom(room);
        conference.AddSpeaker(speaker);
        var presentation = ConferenceFactory.CreatePresentation(room: room, speakers: new[] { speaker });
        conference.AddPresentation(presentation);

        Assert.Throws<InvalidOperationException>(() => conference.RemoveRoom(room.Id));
    }

    [Fact]
    public void RemovePresentation_ExistingPresentation_ShouldRemove()
    {
        var conference = ConferenceFactory.CreatePersistedConference();
        var room = ConferenceFactory.CreateRoom();
        var speaker = ConferenceFactory.CreateSpeaker();
        conference.AddRoom(room);
        conference.AddSpeaker(speaker);
        var presentation = ConferenceFactory.CreatePresentation(room: room, speakers: new[] { speaker });
        conference.AddPresentation(presentation);

        conference.RemovePresentation(presentation.Id);

        Assert.Empty(conference.Presentations);
    }
}
