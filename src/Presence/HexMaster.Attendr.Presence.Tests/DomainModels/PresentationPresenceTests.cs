using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Tests.Factories;

namespace HexMaster.Attendr.Presence.Tests.DomainModels;

public sealed class PresentationPresenceTests
{
    private static readonly DateTimeOffset DefaultStart = DateTimeOffset.UtcNow.AddHours(1);
    private static readonly DateTimeOffset DefaultEnd = DefaultStart.AddHours(1);

    // ── Constructor ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePresentation()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();

        var presentation = new PresentationPresence(profileId, conferenceId, presentationId,
            "Title", "Abstract", "Room A", DefaultStart, DefaultEnd);

        Assert.Equal(profileId, presentation.ProfileId);
        Assert.Equal(conferenceId, presentation.ConferenceId);
        Assert.Equal(presentationId, presentation.PresentationId);
        Assert.Equal("Title", presentation.Title);
        Assert.Equal("Abstract", presentation.Abstract);
        Assert.Equal("Room A", presentation.Room);
        Assert.Equal(DefaultStart, presentation.StartDateTime);
        Assert.Equal(DefaultEnd, presentation.EndDateTime);
        Assert.False(presentation.IsRated);
        Assert.False(presentation.IsFavorite);
        Assert.False(presentation.IsCheckedIn);
        Assert.False(presentation.IsRecommended);
        Assert.False(presentation.IsPreferred);
    }

    [Fact]
    public void Constructor_WithEmptyProfileId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationPresence(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(),
                "Title", "Abstract", "Room", DefaultStart, DefaultEnd));
    }

    [Fact]
    public void Constructor_WithEmptyConferenceId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationPresence(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(),
                "Title", "Abstract", "Room", DefaultStart, DefaultEnd));
    }

    [Fact]
    public void Constructor_WithEmptyPresentationId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationPresence(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty,
                "Title", "Abstract", "Room", DefaultStart, DefaultEnd));
    }

    [Fact]
    public void Constructor_WithWhitespaceTitle_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationPresence(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                "   ", "Abstract", "Room", DefaultStart, DefaultEnd));
    }

    [Fact]
    public void Constructor_WithNullTitle_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PresentationPresence(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                null!, "Abstract", "Room", DefaultStart, DefaultEnd));
    }

    [Fact]
    public void Constructor_WithWhitespaceAbstract_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationPresence(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                "Title", "   ", "Room", DefaultStart, DefaultEnd));
    }

    [Fact]
    public void Constructor_WithWhitespaceRoom_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationPresence(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                "Title", "Abstract", "  ", DefaultStart, DefaultEnd));
    }

    [Fact]
    public void Constructor_WithEndBeforeStart_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationPresence(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                "Title", "Abstract", "Room", DefaultStart, DefaultStart.AddMinutes(-1)));
    }

    [Fact]
    public void Constructor_WithEqualStartAndEnd_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentationPresence(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                "Title", "Abstract", "Room", DefaultStart, DefaultStart));
    }

    [Fact]
    public void Constructor_WithSpeakersAndTopics_ShouldPopulateCollections()
    {
        var speaker = new PresentationSpeaker(Guid.NewGuid(), "Alice", null);
        var topic = new PresentationTopic("csharp", "C#");

        var presentation = new PresentationPresence(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Title", "Abstract", "Room", DefaultStart, DefaultEnd,
            speakers: new[] { speaker }, topics: new[] { topic });

        Assert.Single(presentation.Speakers);
        Assert.Single(presentation.Topics);
    }

    // ── AddSpeaker ───────────────────────────────────────────────────────

    [Fact]
    public void AddSpeaker_WithNewSpeaker_ShouldAddSuccessfully()
    {
        var presentation = PresentationPresenceFactory.Create();
        var speaker = new PresentationSpeaker(Guid.NewGuid(), "Bob", null);

        presentation.AddSpeaker(speaker);

        Assert.Single(presentation.Speakers);
        Assert.Equal("Bob", presentation.Speakers.First().Name);
    }

    [Fact]
    public void AddSpeaker_WithDuplicateSpeakerId_ShouldThrowInvalidOperationException()
    {
        var speakerId = Guid.NewGuid();
        var speaker1 = new PresentationSpeaker(speakerId, "Alice", null);
        var speaker2 = new PresentationSpeaker(speakerId, "Bob", null);
        var presentation = PresentationPresenceFactory.Create(speakers: new[] { speaker1 });

        Assert.Throws<InvalidOperationException>(() => presentation.AddSpeaker(speaker2));
    }

    [Fact]
    public void AddSpeaker_WithNullSpeaker_ShouldThrowArgumentNullException()
    {
        var presentation = PresentationPresenceFactory.Create();

        Assert.Throws<ArgumentNullException>(() => presentation.AddSpeaker(null!));
    }

    // ── UpdatePresentationInfo ────────────────────────────────────────────

    [Fact]
    public void UpdatePresentationInfo_WithValidData_ShouldUpdateProperties()
    {
        var presentation = PresentationPresenceFactory.Create();
        var newStart = DateTimeOffset.UtcNow.AddHours(3);
        var newEnd = newStart.AddHours(1);
        var speakers = new[] { new PresentationSpeaker(Guid.NewGuid(), "Carol", null) };
        var topics = new[] { new PresentationTopic("dotnet", ".NET") };

        presentation.UpdatePresentationInfo("New Title", "New Abstract", "Room B", newStart, newEnd, speakers, topics);

        Assert.Equal("New Title", presentation.Title);
        Assert.Equal("New Abstract", presentation.Abstract);
        Assert.Equal("Room B", presentation.Room);
        Assert.Equal(newStart, presentation.StartDateTime);
        Assert.Equal(newEnd, presentation.EndDateTime);
        Assert.Single(presentation.Speakers);
        Assert.Single(presentation.Topics);
    }

    [Fact]
    public void UpdatePresentationInfo_WithWhitespaceTitle_ShouldThrowArgumentException()
    {
        var presentation = PresentationPresenceFactory.Create();
        var newStart = DateTimeOffset.UtcNow.AddHours(3);
        var newEnd = newStart.AddHours(1);

        Assert.Throws<ArgumentException>(() =>
            presentation.UpdatePresentationInfo("  ", "Abstract", "Room", newStart, newEnd,
                Array.Empty<PresentationSpeaker>(), Array.Empty<PresentationTopic>()));
    }

    [Fact]
    public void UpdatePresentationInfo_WithEndBeforeStart_ShouldThrowArgumentException()
    {
        var presentation = PresentationPresenceFactory.Create();
        var start = DateTimeOffset.UtcNow.AddHours(3);

        Assert.Throws<ArgumentException>(() =>
            presentation.UpdatePresentationInfo("Title", "Abstract", "Room", start, start.AddMinutes(-1),
                Array.Empty<PresentationSpeaker>(), Array.Empty<PresentationTopic>()));
    }

    // ── RatePresentation ─────────────────────────────────────────────────

    [Fact]
    public void RatePresentation_WithValidRating_ShouldSetProperties()
    {
        var presentation = PresentationPresenceFactory.Create();

        presentation.RatePresentation((byte)4, true);

        Assert.True(presentation.IsRated);
        Assert.True(presentation.IsFavorite);
        Assert.Equal((byte)4, presentation.Rating);
    }

    [Fact]
    public void RatePresentation_WithNullRating_ShouldSetIsRatedTrue()
    {
        var presentation = PresentationPresenceFactory.Create();

        presentation.RatePresentation(null, false);

        Assert.True(presentation.IsRated);
        Assert.Null(presentation.Rating);
        Assert.False(presentation.IsFavorite);
    }

    [Fact]
    public void RatePresentation_WithRatingAboveFive_ShouldThrowArgumentException()
    {
        var presentation = PresentationPresenceFactory.Create();

        Assert.Throws<ArgumentException>(() => presentation.RatePresentation((byte)6, false));
    }

    [Fact]
    public void RatePresentation_WithRatingOfFive_ShouldSucceed()
    {
        var presentation = PresentationPresenceFactory.Create();

        presentation.RatePresentation((byte)5, false);

        Assert.Equal((byte)5, presentation.Rating);
    }

    // ── CheckIn / CheckOut ───────────────────────────────────────────────

    [Fact]
    public void CheckIn_ShouldSetIsCheckedInTrueAndTimestamp()
    {
        var presentation = PresentationPresenceFactory.Create();
        var before = DateTimeOffset.UtcNow;

        presentation.CheckIn();

        Assert.True(presentation.IsCheckedIn);
        Assert.NotNull(presentation.CheckedInAt);
        Assert.True(presentation.CheckedInAt >= before);
    }

    [Fact]
    public void CheckOut_ShouldClearIsCheckedInAndTimestamp()
    {
        var presentation = PresentationPresenceFactory.Create(isCheckedIn: true,
            checkedInAt: DateTimeOffset.UtcNow);

        presentation.CheckOut();

        Assert.False(presentation.IsCheckedIn);
        Assert.Null(presentation.CheckedInAt);
    }

    // ── SetAsPreferred / UnsetAsPreferred ────────────────────────────────

    [Fact]
    public void SetAsPreferred_WhenIsFavorite_ShouldSetIsPreferredTrue()
    {
        var presentation = PresentationPresenceFactory.Create(isFavorite: true);

        presentation.SetAsPreferred();

        Assert.True(presentation.IsPreferred);
    }

    [Fact]
    public void SetAsPreferred_WhenNotFavorite_ShouldThrowInvalidOperationException()
    {
        var presentation = PresentationPresenceFactory.Create(isFavorite: false);

        Assert.Throws<InvalidOperationException>(() => presentation.SetAsPreferred());
    }

    [Fact]
    public void UnsetAsPreferred_ShouldSetIsPreferredFalse()
    {
        var presentation = PresentationPresenceFactory.Create(isFavorite: true, isPreferred: true);

        presentation.UnsetAsPreferred();

        Assert.False(presentation.IsPreferred);
    }

    // ── ResetRating ───────────────────────────────────────────────────────

    [Fact]
    public void ResetRating_ShouldClearRatingProperties()
    {
        var presentation = PresentationPresenceFactory.Create(isRated: true, isFavorite: true, rating: 3);

        presentation.ResetRating();

        Assert.False(presentation.IsRated);
        Assert.False(presentation.IsFavorite);
        Assert.Null(presentation.Rating);
    }

    // ── SetRecommended ────────────────────────────────────────────────────

    [Fact]
    public void SetRecommended_True_ShouldSetIsRecommendedTrue()
    {
        var presentation = PresentationPresenceFactory.Create(isRecommended: false);

        presentation.SetRecommended(true);

        Assert.True(presentation.IsRecommended);
    }

    [Fact]
    public void SetRecommended_False_ShouldSetIsRecommendedFalse()
    {
        var presentation = PresentationPresenceFactory.Create(isRecommended: true);

        presentation.SetRecommended(false);

        Assert.False(presentation.IsRecommended);
    }
}
