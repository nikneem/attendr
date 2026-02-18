using Bogus;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events.Groups;
using HexMaster.Attendr.IntegrationEvents.Models;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Events.Groups;

public class GroupEventTests
{
    private readonly Faker _faker = new();

    // ── GroupMemberAddedEvent ──────────────────────────────────────────────

    [Fact]
    public void GroupMemberAddedEvent_EventType_IsCorrect()
    {
        var evt = new GroupMemberAddedEvent();
        Assert.Equal(IntegrationEventTopics.GroupMemberAdded, evt.EventType);
    }

    [Fact]
    public void GroupMemberAddedEvent_Properties_CanBeSet()
    {
        var groupId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        var evt = new GroupMemberAddedEvent
        {
            GroupId = groupId,
            GroupName = "DotNet Enthusiasts",
            ProfileId = profileId,
            Role = "Member"
        };

        Assert.Equal(groupId, evt.GroupId);
        Assert.Equal("DotNet Enthusiasts", evt.GroupName);
        Assert.Equal(profileId, evt.ProfileId);
        Assert.Equal("Member", evt.Role);
    }

    [Fact]
    public void GroupMemberAddedEvent_InheritsFromIntegrationEvent()
    {
        var evt = new GroupMemberAddedEvent();
        Assert.IsAssignableFrom<HexMaster.Attendr.IntegrationEvents.Events.IntegrationEvent>(evt);
    }

    // ── GroupMemberRemovedEvent ────────────────────────────────────────────

    [Fact]
    public void GroupMemberRemovedEvent_EventType_IsCorrect()
    {
        var evt = new GroupMemberRemovedEvent();
        Assert.Equal(IntegrationEventTopics.GroupMemberRemoved, evt.EventType);
    }

    [Fact]
    public void GroupMemberRemovedEvent_Properties_CanBeSet()
    {
        var groupId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        var evt = new GroupMemberRemovedEvent
        {
            GroupId = groupId,
            GroupName = "Cloud Architects",
            ProfileId = profileId
        };

        Assert.Equal(groupId, evt.GroupId);
        Assert.Equal("Cloud Architects", evt.GroupName);
        Assert.Equal(profileId, evt.ProfileId);
    }

    // ── GroupAccessRequestedEvent ──────────────────────────────────────────

    [Fact]
    public void GroupAccessRequestedEvent_EventType_IsCorrect()
    {
        var evt = new GroupAccessRequestedEvent
        {
            GroupId = Guid.NewGuid(),
            GroupName = "Test",
            ProfileId = Guid.NewGuid(),
            ProfileName = "Tester",
            CreatedOn = DateTimeOffset.UtcNow,
            NotificationTargets = new List<NotificationTarget>()
        };
        Assert.Equal(IntegrationEventTopics.GroupAccessRequested, evt.EventType);
    }

    [Fact]
    public void GroupAccessRequestedEvent_Properties_CanBeSet()
    {
        var groupId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var createdOn = DateTimeOffset.UtcNow;
        var targets = new List<NotificationTarget>
        {
            new() { ProfileId = Guid.NewGuid(), ProfileName = "Admin One" },
            new() { ProfileId = Guid.NewGuid(), ProfileName = "Admin Two" }
        };

        var evt = new GroupAccessRequestedEvent
        {
            GroupId = groupId,
            GroupName = "Private Group",
            ProfileId = profileId,
            ProfileName = "New Member",
            CreatedOn = createdOn,
            NotificationTargets = targets
        };

        Assert.Equal(groupId, evt.GroupId);
        Assert.Equal("Private Group", evt.GroupName);
        Assert.Equal(profileId, evt.ProfileId);
        Assert.Equal("New Member", evt.ProfileName);
        Assert.Equal(createdOn, evt.CreatedOn);
        Assert.Equal(2, evt.NotificationTargets.Count);
    }

    // ── ProfilesFollowedConferenceEvent ────────────────────────────────────

    [Fact]
    public void ProfilesFollowedConferenceEvent_EventType_IsCorrect()
    {
        var evt = new ProfilesFollowedConferenceEvent();
        Assert.Equal(IntegrationEventTopics.ProfilesFollowedConference, evt.EventType);
    }

    [Fact]
    public void ProfilesFollowedConferenceEvent_ProfileIds_DefaultsToEmpty()
    {
        var evt = new ProfilesFollowedConferenceEvent();
        Assert.NotNull(evt.ProfileIds);
        Assert.Empty(evt.ProfileIds);
    }

    [Fact]
    public void ProfilesFollowedConferenceEvent_Properties_CanBeSet()
    {
        var conferenceId = Guid.NewGuid();
        var profileIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }.AsReadOnly();

        var evt = new ProfilesFollowedConferenceEvent
        {
            ConferenceId = conferenceId,
            ProfileIds = profileIds
        };

        Assert.Equal(conferenceId, evt.ConferenceId);
        Assert.Equal(3, evt.ProfileIds.Count);
    }
}
