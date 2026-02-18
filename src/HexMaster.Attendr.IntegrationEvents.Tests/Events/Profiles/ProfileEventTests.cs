using Bogus;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Events.Profiles;

public class ProfileEventTests
{
    private readonly Faker _faker = new();

    // ── ProfileCreatedEvent ────────────────────────────────────────────────

    [Fact]
    public void ProfileCreatedEvent_EventType_IsCorrect()
    {
        var evt = new ProfileCreatedEvent();
        Assert.Equal(IntegrationEventTopics.ProfileCreated, evt.EventType);
    }

    [Fact]
    public void ProfileCreatedEvent_OptionalFields_DefaultToNull()
    {
        var evt = new ProfileCreatedEvent();
        Assert.Null(evt.FirstName);
        Assert.Null(evt.LastName);
        Assert.Null(evt.Email);
    }

    [Fact]
    public void ProfileCreatedEvent_Properties_CanBeSet()
    {
        var profileId = Guid.NewGuid().ToString();
        var subjectId = _faker.Random.AlphaNumeric(24);
        var displayName = _faker.Name.FullName();
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var email = _faker.Internet.Email();

        var evt = new ProfileCreatedEvent
        {
            ProfileId = profileId,
            SubjectId = subjectId,
            DisplayName = displayName,
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        Assert.Equal(profileId, evt.ProfileId);
        Assert.Equal(subjectId, evt.SubjectId);
        Assert.Equal(displayName, evt.DisplayName);
        Assert.Equal(firstName, evt.FirstName);
        Assert.Equal(lastName, evt.LastName);
        Assert.Equal(email, evt.Email);
    }

    // ── ProfileUpdatedEvent ────────────────────────────────────────────────

    [Fact]
    public void ProfileUpdatedEvent_EventType_IsCorrect()
    {
        var evt = new ProfileUpdatedEvent();
        Assert.Equal(IntegrationEventTopics.ProfileUpdated, evt.EventType);
    }

    [Fact]
    public void ProfileUpdatedEvent_OptionalFields_DefaultToNull()
    {
        var evt = new ProfileUpdatedEvent();
        Assert.Null(evt.FirstName);
        Assert.Null(evt.LastName);
        Assert.Null(evt.Email);
    }

    [Fact]
    public void ProfileUpdatedEvent_Properties_CanBeSet()
    {
        var evt = new ProfileUpdatedEvent
        {
            ProfileId = "profile-123",
            DisplayName = "Jane Doe",
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com"
        };

        Assert.Equal("profile-123", evt.ProfileId);
        Assert.Equal("Jane Doe", evt.DisplayName);
        Assert.Equal("Jane", evt.FirstName);
        Assert.Equal("Doe", evt.LastName);
        Assert.Equal("jane@example.com", evt.Email);
    }

    // ── ProfileFollowedConferenceEvent ─────────────────────────────────────

    [Fact]
    public void ProfileFollowedConferenceEvent_EventType_IsCorrect()
    {
        var evt = new ProfileFollowedConferenceEvent();
        Assert.Equal(IntegrationEventTopics.ProfileFollowedConference, evt.EventType);
    }

    [Fact]
    public void ProfileFollowedConferenceEvent_Properties_CanBeSet()
    {
        var conferenceId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        var evt = new ProfileFollowedConferenceEvent
        {
            ConferenceId = conferenceId,
            ProfileId = profileId
        };

        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal(profileId, evt.ProfileId);
    }

    // ── ProfileCheckedInEvent ──────────────────────────────────────────────

    [Fact]
    public void ProfileCheckedInEvent_EventType_IsCorrect()
    {
        var evt = new ProfileCheckedInEvent();
        Assert.Equal(IntegrationEventTopics.ProfileCheckedIn, evt.EventType);
    }

    [Fact]
    public void ProfileCheckedInEvent_Properties_CanBeSet()
    {
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var start = DateTime.UtcNow;
        var end = start.AddHours(1);

        var evt = new ProfileCheckedInEvent
        {
            ConferenceId = conferenceId,
            PresentationId = presentationId,
            Title = "Keynote",
            StartDateTime = start,
            EndDateTime = end,
            Room = "Auditorium",
            ProfileId = profileId,
            IsCheckedIn = true
        };

        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal(presentationId, evt.PresentationId);
        Assert.Equal("Keynote", evt.Title);
        Assert.Equal(start, evt.StartDateTime);
        Assert.Equal(end, evt.EndDateTime);
        Assert.Equal("Auditorium", evt.Room);
        Assert.Equal(profileId, evt.ProfileId);
        Assert.True(evt.IsCheckedIn);
    }

    [Fact]
    public void ProfileCheckedInEvent_IsCheckedIn_DefaultsFalse()
    {
        var evt = new ProfileCheckedInEvent();
        Assert.False(evt.IsCheckedIn);
    }

    // ── ProfileConferenceAttendanceChangedEvent ────────────────────────────

    [Fact]
    public void ProfileConferenceAttendanceChangedEvent_EventType_IsCorrect()
    {
        var evt = new ProfileConferenceAttendanceChangedEvent();
        Assert.Equal(IntegrationEventTopics.ProfileConferenceAttendanceChanged, evt.EventType);
    }

    [Fact]
    public void ProfileConferenceAttendanceChangedEvent_Properties_CanBeSet()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        var evt = new ProfileConferenceAttendanceChangedEvent
        {
            ProfileId = profileId,
            ConferenceId = conferenceId,
            ConferenceName = "DotNetConf",
            IsAttending = true
        };

        Assert.Equal(profileId, evt.ProfileId);
        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal("DotNetConf", evt.ConferenceName);
        Assert.True(evt.IsAttending);
    }

    // ── ProfileTopicInterestEvent ──────────────────────────────────────────

    [Fact]
    public void ProfileTopicInterestEvent_EventType_IsCorrect()
    {
        var evt = new ProfileTopicInterestEvent
        {
            ProfileId = "p1",
            TopicKey = "csharp",
            TopicName = "C#",
            Weight = 80
        };
        Assert.Equal(IntegrationEventTopics.ProfileTopicInterest, evt.EventType);
    }

    [Fact]
    public void ProfileTopicInterestEvent_RequiredProperties_MustBeSet()
    {
        var evt = new ProfileTopicInterestEvent
        {
            ProfileId = "profile-1",
            TopicKey = "dotnet",
            TopicName = ".NET",
            Weight = 90
        };

        Assert.Equal("profile-1", evt.ProfileId);
        Assert.Equal("dotnet", evt.TopicKey);
        Assert.Equal(".NET", evt.TopicName);
        Assert.Equal(90, evt.Weight);
    }

    [Fact]
    public void ProfileTopicInterestEvent_IsManual_DefaultsFalse()
    {
        var evt = new ProfileTopicInterestEvent
        {
            ProfileId = "p1",
            TopicKey = "azure",
            TopicName = "Azure",
            Weight = 50
        };

        Assert.False(evt.IsManual);
    }

    [Fact]
    public void ProfileTopicInterestEvent_IsManual_CanBeSetToTrue()
    {
        var evt = new ProfileTopicInterestEvent
        {
            ProfileId = "p1",
            TopicKey = "azure",
            TopicName = "Azure",
            Weight = 50,
            IsManual = true
        };

        Assert.True(evt.IsManual);
    }

    // ── ProfileTopicsChangedEvent ──────────────────────────────────────────

    [Fact]
    public void ProfileTopicsChangedEvent_EventType_IsCorrect()
    {
        var evt = new ProfileTopicsChangedEvent();
        Assert.Equal(IntegrationEventTopics.ProfileTopicsChanged, evt.EventType);
    }

    [Fact]
    public void ProfileTopicsChangedEvent_Topics_DefaultsToEmpty()
    {
        var evt = new ProfileTopicsChangedEvent();
        Assert.NotNull(evt.Topics);
        Assert.Empty(evt.Topics);
    }

    [Fact]
    public void ProfileTopicsChangedEvent_Properties_CanBeSet()
    {
        var topics = new List<ProfileTopicInfo>
        {
            new("dotnet", ".NET", 90),
            new("azure", "Azure", 75)
        };

        var evt = new ProfileTopicsChangedEvent
        {
            ProfileId = "profile-42",
            Topics = topics
        };

        Assert.Equal("profile-42", evt.ProfileId);
        Assert.Equal(2, evt.Topics.Count);
        Assert.Equal("dotnet", evt.Topics[0].TopicKey);
        Assert.Equal(".NET", evt.Topics[0].TopicName);
        Assert.Equal(90, evt.Topics[0].Weight);
    }
}
