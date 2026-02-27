using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Tests.Factories;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

/// <summary>
/// Tests for Presentation mutation methods that are not covered in PresentationTests.cs.
/// </summary>
public class PresentationMutationTests
{
    private static Presentation CreatePresentation(
        IEnumerable<Speaker>? extraSpeakers = null,
        IEnumerable<PresentationTopic>? topics = null)
    {
        var room = ConferenceFactory.CreateRoom(null, null);
        var speaker = ConferenceFactory.CreateSpeaker(null, null);
        var speakers = extraSpeakers != null
            ? new List<Speaker> { speaker }.Concat(extraSpeakers).ToList()
            : new List<Speaker> { speaker };
        return ConferenceFactory.CreatePresentation(null, null, null, null, room, speakers);
    }

    // ── UpdateDetails ───────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_WithNewTitle_ShouldUpdateTitle()
    {
        var room = ConferenceFactory.CreateRoom(null, null);
        var speaker = ConferenceFactory.CreateSpeaker(null, null);
        var p = ConferenceFactory.CreatePresentation("Original Title", null, null, null, room, new[] { speaker });

        p.UpdateDetails("New Title", p.Abstract, p.StartDateTime, p.EndDateTime);

        Assert.Equal("New Title", p.Title);
    }

    [Fact]
    public void UpdateDetails_WithNewAbstract_ShouldSetIsAnalysedFalse()
    {
        var room = ConferenceFactory.CreateRoom(null, null);
        var speaker = ConferenceFactory.CreateSpeaker(null, null);
        var p = Presentation.FromPersisted(
            Guid.NewGuid(), "Title", "Old Abstract",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1),
            room, new[] { speaker }, isAnalysed: true);

        p.UpdateDetails(p.Title, "New Abstract", p.StartDateTime, p.EndDateTime);

        Assert.Equal("New Abstract", p.Abstract);
        Assert.False(p.IsAnalysed);
    }

    [Fact]
    public void UpdateDetails_WithSameValues_ShouldNotChangeState()
    {
        var room = ConferenceFactory.CreateRoom(null, null);
        var speaker = ConferenceFactory.CreateSpeaker(null, null);
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);
        var p = ConferenceFactory.CreatePresentation("Title", "Abstract", start, end, room, new[] { speaker });

        // Update with same values — no property changes expected
        p.UpdateDetails("Title", "Abstract", start, end);

        Assert.Equal("Title", p.Title);
        Assert.Equal("Abstract", p.Abstract);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateDetails_WithEmptyTitle_ShouldThrowArgumentException(string? title)
    {
        var p = CreatePresentation();
        Assert.ThrowsAny<ArgumentException>(() =>
            p.UpdateDetails(title!, p.Abstract, p.StartDateTime, p.EndDateTime));
    }

    [Fact]
    public void UpdateDetails_WithEndBeforeStart_ShouldThrowArgumentException()
    {
        var p = CreatePresentation();
        var start = p.StartDateTime;
        Assert.Throws<ArgumentException>(() =>
            p.UpdateDetails(p.Title, p.Abstract, start, start.AddMinutes(-1)));
    }

    // ── ChangeRoom ──────────────────────────────────────────────────────────

    [Fact]
    public void ChangeRoom_WithDifferentRoom_ShouldUpdateRoom()
    {
        var p = CreatePresentation();
        var newRoom = ConferenceFactory.CreateRoom("New Room", 50);

        p.ChangeRoom(newRoom);

        Assert.Equal(newRoom.Id, p.Room.Id);
    }

    [Fact]
    public void ChangeRoom_WithSameRoom_ShouldNotChange()
    {
        var p = CreatePresentation();
        var originalRoomId = p.Room.Id;

        p.ChangeRoom(p.Room);

        Assert.Equal(originalRoomId, p.Room.Id);
    }

    [Fact]
    public void ChangeRoom_WithNullRoom_ShouldThrowArgumentNullException()
    {
        var p = CreatePresentation();
        Assert.Throws<ArgumentNullException>(() => p.ChangeRoom(null!));
    }

    // ── AddSpeaker ──────────────────────────────────────────────────────────

    [Fact]
    public void AddSpeaker_WithNewSpeaker_ShouldAddToCollection()
    {
        var p = CreatePresentation();
        var newSpeaker = ConferenceFactory.CreateSpeaker("Bob", null);

        p.AddSpeaker(newSpeaker);

        Assert.Contains(p.Speakers, s => s.Id == newSpeaker.Id);
    }

    [Fact]
    public void AddSpeaker_WithNullSpeaker_ShouldThrowArgumentNullException()
    {
        var p = CreatePresentation();
        Assert.Throws<ArgumentNullException>(() => p.AddSpeaker(null!));
    }

    [Fact]
    public void AddSpeaker_WithDuplicateSpeaker_ShouldThrowInvalidOperationException()
    {
        var p = CreatePresentation();
        var existingSpeaker = p.Speakers.First();

        Assert.Throws<InvalidOperationException>(() => p.AddSpeaker(existingSpeaker));
    }

    // ── RemoveSpeaker ───────────────────────────────────────────────────────

    [Fact]
    public void RemoveSpeaker_WithExtraSpeaker_ShouldRemoveIt()
    {
        var extra = ConferenceFactory.CreateSpeaker("Extra", null);
        var p = CreatePresentation(new[] { extra });
        Assert.Equal(2, p.Speakers.Count);

        p.RemoveSpeaker(extra.Id);

        Assert.Single(p.Speakers);
        Assert.DoesNotContain(p.Speakers, s => s.Id == extra.Id);
    }

    [Fact]
    public void RemoveSpeaker_LastSpeaker_ShouldThrowInvalidOperationException()
    {
        var p = CreatePresentation();
        Assert.Single(p.Speakers);

        Assert.Throws<InvalidOperationException>(() => p.RemoveSpeaker(p.Speakers.First().Id));
    }

    [Fact]
    public void RemoveSpeaker_WithEmptyGuid_ShouldThrowArgumentException()
    {
        var p = CreatePresentation();
        Assert.Throws<ArgumentException>(() => p.RemoveSpeaker(Guid.Empty));
    }

    [Fact]
    public void RemoveSpeaker_WithNonExistentId_ShouldThrowInvalidOperationException()
    {
        var extra = ConferenceFactory.CreateSpeaker("Extra", null);
        var p = CreatePresentation(new[] { extra });

        Assert.Throws<InvalidOperationException>(() => p.RemoveSpeaker(Guid.NewGuid()));
    }

    // ── AddTopic ────────────────────────────────────────────────────────────

    [Fact]
    public void AddTopic_WithNewTopic_ShouldAddToTopics()
    {
        var p = CreatePresentation();
        var topic = new PresentationTopic("dotnet", ".NET");

        p.AddTopic(topic);

        Assert.Contains(p.Topics, t => t.Key == "dotnet");
    }

    [Fact]
    public void AddTopic_WithNullTopic_ShouldThrowArgumentNullException()
    {
        var p = CreatePresentation();
        Assert.Throws<ArgumentNullException>(() => p.AddTopic(null!));
    }

    [Fact]
    public void AddTopic_WithDuplicateKey_ShouldThrowInvalidOperationException()
    {
        var p = CreatePresentation();
        p.AddTopic(new PresentationTopic("dotnet", ".NET"));

        Assert.Throws<InvalidOperationException>(() => p.AddTopic(new PresentationTopic("dotnet", ".NET different name")));
    }

    [Fact]
    public void AddTopic_WithDuplicateCaseInsensitiveKey_ShouldThrowInvalidOperationException()
    {
        var p = CreatePresentation();
        p.AddTopic(new PresentationTopic("DotNet", ".NET"));

        Assert.Throws<InvalidOperationException>(() => p.AddTopic(new PresentationTopic("dotnet", ".NET")));
    }

    // ── RemoveTopic ─────────────────────────────────────────────────────────

    [Fact]
    public void RemoveTopic_WithExistingKey_ShouldRemoveTopic()
    {
        var p = CreatePresentation();
        p.AddTopic(new PresentationTopic("dotnet", ".NET"));
        p.AddTopic(new PresentationTopic("azure", "Azure"));

        p.RemoveTopic("dotnet");

        Assert.DoesNotContain(p.Topics, t => t.Key == "dotnet");
        Assert.Contains(p.Topics, t => t.Key == "azure");
    }

    [Fact]
    public void RemoveTopic_WithNonExistentKey_ShouldThrowInvalidOperationException()
    {
        var p = CreatePresentation();
        Assert.Throws<InvalidOperationException>(() => p.RemoveTopic("nonexistent"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RemoveTopic_WithEmptyKey_ShouldThrowArgumentException(string? key)
    {
        var p = CreatePresentation();
        Assert.ThrowsAny<ArgumentException>(() => p.RemoveTopic(key!));
    }

    // ── UpdateTopics ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateTopics_WithNewSet_ShouldReplaceTopics()
    {
        var p = CreatePresentation();
        p.AddTopic(new PresentationTopic("old", "Old"));

        var newTopics = new List<PresentationTopic>
        {
            new("dotnet", ".NET"),
            new("azure", "Azure")
        };

        p.UpdateTopics(newTopics);

        Assert.Equal(2, p.Topics.Count);
        Assert.Contains(p.Topics, t => t.Key == "dotnet");
        Assert.Contains(p.Topics, t => t.Key == "azure");
        Assert.DoesNotContain(p.Topics, t => t.Key == "old");
    }

    [Fact]
    public void UpdateTopics_WithSameSet_ShouldNotChangeState()
    {
        var p = CreatePresentation();
        var topic = new PresentationTopic("dotnet", ".NET");
        p.AddTopic(topic);

        // Same topics → no-op
        p.UpdateTopics(new List<PresentationTopic> { topic });

        Assert.Single(p.Topics);
    }

    [Fact]
    public void UpdateTopics_WithNullTopics_ShouldThrowArgumentNullException()
    {
        var p = CreatePresentation();
        Assert.Throws<ArgumentNullException>(() => p.UpdateTopics(null!));
    }
}
