using Bogus;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Models;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Events.Conferences;

public class ConferenceEventTests
{
    private readonly Faker _faker = new();

    // ── ConferenceCreatedEvent ──────────────────────────────────────────────

    [Fact]
    public void ConferenceCreatedEvent_EventType_IsCorrect()
    {
        var evt = new ConferenceCreatedEvent();
        Assert.Equal(IntegrationEventTopics.ConferenceCreated, evt.EventType);
    }

    [Fact]
    public void ConferenceCreatedEvent_InheritsFromIntegrationEvent()
    {
        var evt = new ConferenceCreatedEvent();
        Assert.IsAssignableFrom<HexMaster.Attendr.IntegrationEvents.Events.IntegrationEvent>(evt);
    }

    [Fact]
    public void ConferenceCreatedEvent_Properties_CanBeSet()
    {
        var conferenceId = Guid.NewGuid();
        var title = _faker.Lorem.Word();
        var city = _faker.Address.City();
        var country = _faker.Address.Country();
        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));

        var evt = new ConferenceCreatedEvent
        {
            ConferenceId = conferenceId,
            Title = title,
            City = city,
            Country = country,
            StartDate = startDate,
            EndDate = endDate
        };

        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal(title, evt.Title);
        Assert.Equal(city, evt.City);
        Assert.Equal(country, evt.Country);
        Assert.Equal(startDate, evt.StartDate);
        Assert.Equal(endDate, evt.EndDate);
    }

    // ── ConferenceUpdatedEvent ──────────────────────────────────────────────

    [Fact]
    public void ConferenceUpdatedEvent_EventType_IsCorrect()
    {
        var evt = new ConferenceUpdatedEvent();
        Assert.Equal(IntegrationEventTopics.ConferenceUpdated, evt.EventType);
    }

    [Fact]
    public void ConferenceUpdatedEvent_ImageUrl_IsNullableAndDefaultsToNull()
    {
        var evt = new ConferenceUpdatedEvent();
        Assert.Null(evt.ImageUrl);
    }

    [Fact]
    public void ConferenceUpdatedEvent_Properties_CanBeSet()
    {
        var conferenceId = Guid.NewGuid();
        var imageUrl = _faker.Internet.Url();

        var evt = new ConferenceUpdatedEvent
        {
            ConferenceId = conferenceId,
            Title = "TechConf 2024",
            City = "Amsterdam",
            Country = "Netherlands",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            ImageUrl = imageUrl
        };

        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal("TechConf 2024", evt.Title);
        Assert.Equal(imageUrl, evt.ImageUrl);
    }

    // ── ConferencePresentationsImportedEvent ───────────────────────────────

    [Fact]
    public void ConferencePresentationsImportedEvent_EventType_IsCorrect()
    {
        var evt = new ConferencePresentationsImportedEvent();
        Assert.Equal(IntegrationEventTopics.ConferencePresentationsImported, evt.EventType);
    }

    [Fact]
    public void ConferencePresentationsImportedEvent_ProfileIds_DefaultsToEmpty()
    {
        var evt = new ConferencePresentationsImportedEvent();
        Assert.NotNull(evt.ProfileIds);
        Assert.Empty(evt.ProfileIds);
    }

    [Fact]
    public void ConferencePresentationsImportedEvent_Properties_CanBeSet()
    {
        var conferenceId = Guid.NewGuid();
        var profileIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        const int presentationsCount = 42;

        var evt = new ConferencePresentationsImportedEvent
        {
            ConferenceId = conferenceId,
            ConferenceName = "NDC Oslo",
            ProfileIds = profileIds.AsReadOnly(),
            PresentationsCount = presentationsCount
        };

        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal("NDC Oslo", evt.ConferenceName);
        Assert.Equal(2, evt.ProfileIds.Count);
        Assert.Equal(presentationsCount, evt.PresentationsCount);
    }

    // ── PresentationUpdatedEvent ───────────────────────────────────────────

    [Fact]
    public void PresentationUpdatedEvent_EventType_IsCorrect()
    {
        var evt = new PresentationUpdatedEvent();
        Assert.Equal(IntegrationEventTopics.PresentationUpdated, evt.EventType);
    }

    [Fact]
    public void PresentationUpdatedEvent_Speakers_DefaultsToEmpty()
    {
        var evt = new PresentationUpdatedEvent();
        Assert.NotNull(evt.Speakers);
        Assert.Empty(evt.Speakers);
    }

    [Fact]
    public void PresentationUpdatedEvent_Topics_DefaultsToEmpty()
    {
        var evt = new PresentationUpdatedEvent();
        Assert.NotNull(evt.Topics);
        Assert.Empty(evt.Topics);
    }

    [Fact]
    public void PresentationUpdatedEvent_Properties_CanBeSet()
    {
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var speakers = new List<SpeakerDto> { new SpeakerDto(Guid.NewGuid(), "Jane Doe", "https://example.com/jane.jpg") }.AsReadOnly();
        var topics = new List<PresentationTopicDto>
        {
            new("csharp", "C#"),
            new("dotnet", ".NET")
        }.AsReadOnly();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);

        var evt = new PresentationUpdatedEvent
        {
            ConferenceId = conferenceId,
            PresentationId = presentationId,
            Title = "Deep Dive into .NET 10",
            Abstract = "An abstract",
            StartDateTime = start,
            EndDateTime = end,
            RoomId = roomId,
            RoomName = "Room A",
            Speakers = speakers,
            Topics = topics,
            ExternalId = "ext-123",
            IsScheduleChanged = true
        };

        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal(presentationId, evt.PresentationId);
        Assert.Equal("Deep Dive into .NET 10", evt.Title);
        Assert.Equal("An abstract", evt.Abstract);
        Assert.Equal(start, evt.StartDateTime);
        Assert.Equal(end, evt.EndDateTime);
        Assert.Equal(roomId, evt.RoomId);
        Assert.Equal("Room A", evt.RoomName);
        Assert.Single(evt.Speakers);
        Assert.Equal("Jane Doe", evt.Speakers.First().Name);
        Assert.Equal(2, evt.Topics.Count);
        Assert.Equal("ext-123", evt.ExternalId);
        Assert.True(evt.IsScheduleChanged);
    }

    // ── PresentationScheduleChangeEvent ───────────────────────────────────

    [Fact]
    public void PresentationScheduleChangeEvent_EventType_IsCorrect()
    {
        var evt = new PresentationScheduleChangeEvent();
        Assert.Equal(IntegrationEventTopics.PresentationScheduleChanged, evt.EventType);
    }

    [Fact]
    public void PresentationScheduleChangeEvent_Properties_CanBeSet()
    {
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);

        var evt = new PresentationScheduleChangeEvent
        {
            ConferenceId = conferenceId,
            PresentationId = presentationId,
            ProfileId = profileId,
            Title = "My Talk",
            Abstract = "Abstract text",
            Room = "Hall B",
            StartDateTime = start,
            EndDateTime = end
        };

        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal(presentationId, evt.PresentationId);
        Assert.Equal(profileId, evt.ProfileId);
        Assert.Equal("My Talk", evt.Title);
        Assert.Equal("Abstract text", evt.Abstract);
        Assert.Equal("Hall B", evt.Room);
        Assert.Equal(start, evt.StartDateTime);
        Assert.Equal(end, evt.EndDateTime);
    }
}
