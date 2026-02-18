using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Constants;

public class IntegrationEventTopicsTests
{
    [Fact]
    public void ConferenceCreated_HasExpectedValue()
        => Assert.Equal("conference.created", IntegrationEventTopics.ConferenceCreated);

    [Fact]
    public void ConferenceUpdated_HasExpectedValue()
        => Assert.Equal("conference.updated", IntegrationEventTopics.ConferenceUpdated);

    [Fact]
    public void ProfileCreated_HasExpectedValue()
        => Assert.Equal("profile.created", IntegrationEventTopics.ProfileCreated);

    [Fact]
    public void ProfileUpdated_HasExpectedValue()
        => Assert.Equal("profile.updated", IntegrationEventTopics.ProfileUpdated);

    [Fact]
    public void ProfileFollowedConference_HasExpectedValue()
        => Assert.Equal("profile.followed.conference", IntegrationEventTopics.ProfileFollowedConference);

    [Fact]
    public void ProfilesFollowedConference_HasExpectedValue()
        => Assert.Equal("profiles.followed.conference", IntegrationEventTopics.ProfilesFollowedConference);

    [Fact]
    public void PresentationUpdated_HasExpectedValue()
        => Assert.Equal("presentation.updated", IntegrationEventTopics.PresentationUpdated);

    [Fact]
    public void ConferencePresentationsImported_HasExpectedValue()
        => Assert.Equal("conference.presentations-imported", IntegrationEventTopics.ConferencePresentationsImported);

    [Fact]
    public void PresentationScheduleChanged_HasExpectedValue()
        => Assert.Equal("presentation.schedule-changed", IntegrationEventTopics.PresentationScheduleChanged);

    [Fact]
    public void ProfileCheckedIn_HasExpectedValue()
        => Assert.Equal("profile.checked-in", IntegrationEventTopics.ProfileCheckedIn);

    [Fact]
    public void ProfileConferenceAttendanceChanged_HasExpectedValue()
        => Assert.Equal("profile.conference-attendance-changed", IntegrationEventTopics.ProfileConferenceAttendanceChanged);

    [Fact]
    public void ProfileTopicInterest_HasExpectedValue()
        => Assert.Equal("profile.topic-interest", IntegrationEventTopics.ProfileTopicInterest);

    [Fact]
    public void ProfileTopicsChanged_HasExpectedValue()
        => Assert.Equal("profile.topics-changed", IntegrationEventTopics.ProfileTopicsChanged);

    [Fact]
    public void GroupMemberAdded_HasExpectedValue()
        => Assert.Equal("group.member-added", IntegrationEventTopics.GroupMemberAdded);

    [Fact]
    public void GroupMemberRemoved_HasExpectedValue()
        => Assert.Equal("group.member-removed", IntegrationEventTopics.GroupMemberRemoved);

    [Fact]
    public void GroupAccessRequested_HasExpectedValue()
        => Assert.Equal("group.access-requested", IntegrationEventTopics.GroupAccessRequested);

    [Fact]
    public void AllTopics_AreUnique()
    {
        var topics = new[]
        {
            IntegrationEventTopics.ConferenceCreated,
            IntegrationEventTopics.ConferenceUpdated,
            IntegrationEventTopics.ProfileCreated,
            IntegrationEventTopics.ProfileUpdated,
            IntegrationEventTopics.ProfileFollowedConference,
            IntegrationEventTopics.ProfilesFollowedConference,
            IntegrationEventTopics.PresentationUpdated,
            IntegrationEventTopics.ConferencePresentationsImported,
            IntegrationEventTopics.PresentationScheduleChanged,
            IntegrationEventTopics.ProfileCheckedIn,
            IntegrationEventTopics.ProfileConferenceAttendanceChanged,
            IntegrationEventTopics.ProfileTopicInterest,
            IntegrationEventTopics.ProfileTopicsChanged,
            IntegrationEventTopics.GroupMemberAdded,
            IntegrationEventTopics.GroupMemberRemoved,
            IntegrationEventTopics.GroupAccessRequested
        };

        Assert.Equal(topics.Length, topics.Distinct().Count());
    }
}
