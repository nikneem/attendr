using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Tests.Factories;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

/// <summary>
/// Tests for Presentation constructor validation and UpdateDetails with new datetimes.
/// </summary>
public class PresentationConstructorValidationTests
{
    private static readonly Room DefaultRoom = Room.Create("Test Room", 100);
    private static readonly Speaker DefaultSpeaker = Speaker.Create("Test Speaker");
    private static readonly DateTimeOffset DefaultStart = DateTimeOffset.UtcNow.AddDays(1);
    private static readonly DateTimeOffset DefaultEnd = DefaultStart.AddHours(1);

    [Fact]
    public void FromPersisted_WithEmptyGuid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Presentation.FromPersisted(Guid.Empty, "Title", "Abstract", DefaultStart, DefaultEnd, DefaultRoom, new[] { DefaultSpeaker }));
    }

    [Fact]
    public void FromPersisted_WithNullSpeakerInList_ThrowsArgumentException()
    {
        var speakers = new Speaker[] { DefaultSpeaker, null! };
        Assert.Throws<ArgumentException>(() =>
            Presentation.FromPersisted(Guid.NewGuid(), "Title", "Abstract", DefaultStart, DefaultEnd, DefaultRoom, speakers));
    }

    [Fact]
    public void FromPersisted_WithNonNullTopics_AddsTopics()
    {
        var topics = new List<PresentationTopic>
        {
            new PresentationTopic("azure-functions", "Azure Functions"),
            new PresentationTopic("serverless", "Serverless")
        };

        var presentation = Presentation.FromPersisted(
            Guid.NewGuid(), "Title", "Abstract", DefaultStart, DefaultEnd,
            DefaultRoom, new[] { DefaultSpeaker }, null, topics);

        Assert.Equal(2, presentation.Topics.Count);
        Assert.Contains(presentation.Topics, t => t.Key == "azure-functions");
    }

    [Fact]
    public void UpdateDetails_WithNewStartDateTime_UpdatesStartDateTime()
    {
        var room = ConferenceFactory.CreateRoom();
        var speaker = ConferenceFactory.CreateSpeaker();
        var originalStart = DateTimeOffset.UtcNow.AddDays(1);
        var originalEnd = originalStart.AddHours(1);
        var p = ConferenceFactory.CreatePresentation(null, null, originalStart, originalEnd, room, new[] { speaker });

        var newStart = originalStart.AddHours(2);
        var newEnd = newStart.AddHours(1);

        p.UpdateDetails(p.Title, p.Abstract, newStart, newEnd);

        Assert.Equal(newStart, p.StartDateTime);
        Assert.Equal(newEnd, p.EndDateTime);
    }

    [Fact]
    public void UpdateDetails_WithNewEndDateTime_UpdatesEndDateTime()
    {
        var room = ConferenceFactory.CreateRoom();
        var speaker = ConferenceFactory.CreateSpeaker();
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        var p = ConferenceFactory.CreatePresentation(null, null, start, end, room, new[] { speaker });

        var newEnd = end.AddHours(1); // Extend for 1 more hour

        p.UpdateDetails(p.Title, p.Abstract, start, newEnd);

        Assert.Equal(newEnd, p.EndDateTime);
    }
}
