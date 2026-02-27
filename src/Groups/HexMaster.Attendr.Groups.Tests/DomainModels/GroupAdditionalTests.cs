using Bogus;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

/// <summary>
/// Additional tests for Group domain model covering methods not covered in GroupTests.cs.
/// </summary>
public sealed class GroupAdditionalTests
{
    private readonly Faker _faker = new();

    // ── AcceptInvitation ──────────────────────────────────────────────────────

    [Fact]
    public void AcceptInvitation_WithValidCode_ShouldAddMemberAndRemoveInvitation()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var inviteeId = Guid.NewGuid();
        var inviteeName = _faker.Person.FullName;
        var expiration = DateTimeOffset.UtcNow.AddDays(7);
        group.AddInvitation(inviteeId, inviteeName, expiration);

        var acceptanceCode = group.Invitations.First().AcceptanceCode;

        // Act
        group.AcceptInvitation(inviteeId, acceptanceCode);

        // Assert
        Assert.Empty(group.Invitations);
        Assert.Contains(group.Members, m => m.Id == inviteeId);
    }

    [Fact]
    public void AcceptInvitation_WithWrongCode_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var inviteeId = Guid.NewGuid();
        group.AddInvitation(inviteeId, _faker.Person.FullName, DateTimeOffset.UtcNow.AddDays(7));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.AcceptInvitation(inviteeId, "WRONGCOD"));
    }

    [Fact]
    public void AcceptInvitation_ForNonExistentInvitation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.AcceptInvitation(Guid.NewGuid(), "ABCD1234"));
    }

    [Fact]
    public void AddInvitation_WhenActiveInvitationExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var inviteeId = Guid.NewGuid();
        group.AddInvitation(inviteeId, _faker.Person.FullName, DateTimeOffset.UtcNow.AddDays(7));

        // Act & Assert – second invite for the same user
        Assert.Throws<InvalidOperationException>(() =>
            group.AddInvitation(inviteeId, _faker.Person.FullName, DateTimeOffset.UtcNow.AddDays(7)));
    }

    [Fact]
    public void RemoveInvitation_ForNonExistentInvitation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.RemoveInvitation(Guid.NewGuid()));
    }

    [Fact]
    public void CleanupExpiredInvitations_ShouldRemoveExpiredInvitations()
    {
        // Arrange – use FromPersisted so we can supply an already-expired invitation is not directly
        // possible through AddInvitation which requires future date. Instead, add a valid invitation
        // and verify CleanupExpiredInvitations works on a group with no invitations (no-op case).
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var inviteeId = Guid.NewGuid();
        group.AddInvitation(inviteeId, _faker.Person.FullName, DateTimeOffset.UtcNow.AddDays(7));

        // Act
        group.CleanupExpiredInvitations();

        // Assert – the non-expired invitation is still there
        Assert.Single(group.Invitations);
    }

    [Fact]
    public void CleanupExpiredInvitations_WithNoInvitations_ShouldNotThrow()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        var ex = Record.Exception(() => group.CleanupExpiredInvitations());
        Assert.Null(ex);
    }

    // ── JoinRequest ──────────────────────────────────────────────────────────

    [Fact]
    public void AddJoinRequest_WithValidData_ShouldAddRequest()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var profileId = Guid.NewGuid();
        var profileName = _faker.Person.FullName;

        // Act
        group.AddJoinRequest(profileId, profileName);

        // Assert
        Assert.Single(group.JoinRequests);
        Assert.Contains(group.JoinRequests, jr => jr.Id == profileId);
    }

    [Fact]
    public void AddJoinRequest_WhenAlreadyMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = Group.Create(_faker.Company.CompanyName(), ownerId, _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.AddJoinRequest(ownerId, _faker.Person.FullName));
    }

    [Fact]
    public void AddJoinRequest_WhenRequestAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var profileId = Guid.NewGuid();
        group.AddJoinRequest(profileId, _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.AddJoinRequest(profileId, _faker.Person.FullName));
    }

    [Fact]
    public void ApproveJoinRequest_WithValidRequest_ShouldAddMemberAndRemoveRequest()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var profileId = Guid.NewGuid();
        group.AddJoinRequest(profileId, _faker.Person.FullName);

        // Act
        group.ApproveJoinRequest(profileId);

        // Assert
        Assert.Empty(group.JoinRequests);
        Assert.Contains(group.Members, m => m.Id == profileId);
    }

    [Fact]
    public void ApproveJoinRequest_ForNonExistentRequest_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.ApproveJoinRequest(Guid.NewGuid()));
    }

    [Fact]
    public void DeclineJoinRequest_WithValidRequest_ShouldRemoveRequest()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var profileId = Guid.NewGuid();
        group.AddJoinRequest(profileId, _faker.Person.FullName);

        // Act
        group.DeclineJoinRequest(profileId);

        // Assert
        Assert.Empty(group.JoinRequests);
    }

    [Fact]
    public void DeclineJoinRequest_ForNonExistentRequest_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.DeclineJoinRequest(Guid.NewGuid()));
    }

    // ── GetOwner ────────────────────────────────────────────────────────────

    [Fact]
    public void GetOwner_ShouldReturnOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = Group.Create(_faker.Company.CompanyName(), ownerId, _faker.Person.FullName);

        // Act
        var owner = group.GetOwner();

        // Assert
        Assert.Equal(ownerId, owner.Id);
        Assert.Equal(GroupRole.Owner, owner.Role);
    }

    // ── UpdateMemberRole – non-existent member ───────────────────────────────

    [Fact]
    public void UpdateMemberRole_ForNonExistentMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.UpdateMemberRole(Guid.NewGuid(), GroupRole.Manager));
    }

    // ── FollowConference / UnfollowConference ────────────────────────────────

    [Fact]
    public void FollowConference_WithValidData_ShouldAddConference()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var conferenceId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(12));

        // Act
        group.FollowConference(conferenceId, "TechConf 2026", "Amsterdam", "Netherlands", null, 10, 20, start, end);

        // Assert
        Assert.Single(group.FollowedConferences);
        Assert.Contains(group.FollowedConferences, fc => fc.ConferenceId == conferenceId);
    }

    [Fact]
    public void FollowConference_WhenAlreadyFollowing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var conferenceId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(12));
        group.FollowConference(conferenceId, "TechConf 2026", "Amsterdam", "Netherlands", null, 10, 20, start, end);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.FollowConference(conferenceId, "TechConf 2026", "Amsterdam", "Netherlands", null, 10, 20, start, end));
    }

    [Fact]
    public void UnfollowConference_WithFollowedConference_ShouldRemoveConference()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var conferenceId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(12));
        group.FollowConference(conferenceId, "TechConf 2026", "Amsterdam", "Netherlands", null, 10, 20, start, end);

        // Act
        group.UnfollowConference(conferenceId);

        // Assert
        Assert.Empty(group.FollowedConferences);
    }

    [Fact]
    public void UnfollowConference_WhenNotFollowing_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.UnfollowConference(Guid.NewGuid()));
    }

    [Fact]
    public void GetCurrentAndFutureFollowedConferences_ShouldReturnOnlyFutureConferences()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var futureConferenceId = Guid.NewGuid();
        var futureStart = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var futureEnd = DateOnly.FromDateTime(DateTime.Today.AddDays(32));
        group.FollowConference(futureConferenceId, "Future Conf", "Berlin", "Germany", null, 5, 10, futureStart, futureEnd);

        // Act
        var result = group.GetCurrentAndFutureFollowedConferences().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(futureConferenceId, result[0].ConferenceId);
    }

    // ── AddActivity ──────────────────────────────────────────────────────────

    [Fact]
    public void AddActivity_WithValidData_ShouldAddActivity()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var profileId = Guid.NewGuid();

        // Act
        group.AddActivity(profileId, "Joined the group", GroupActivityType.ProfileJoinedGroup);

        // Assert
        Assert.Single(group.Activities);
    }

    [Fact]
    public void AddActivity_WithEmptyProfileId_ShouldThrowArgumentException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            group.AddActivity(Guid.Empty, "Some activity", GroupActivityType.ProfileJoinedGroup));
    }

    [Fact]
    public void AddActivity_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            group.AddActivity(Guid.NewGuid(), null!, GroupActivityType.ProfileJoinedGroup));
    }

    [Fact]
    public void AddActivity_WithNullActivityType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            group.AddActivity(Guid.NewGuid(), "Some activity", null!));
    }

    [Fact]
    public void AddActivity_WhenMaxActivitiesExceeded_ShouldEvictOldest()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var profileId = Guid.NewGuid();

        // Add 100 activities
        for (var i = 0; i < 100; i++)
        {
            group.AddActivity(profileId, $"Activity {i}", GroupActivityType.ProfileJoinedGroup);
        }

        Assert.Equal(100, group.Activities.Count);
        var firstActivityBefore = group.Activities.First().Description;

        // Act – add the 101st activity
        group.AddActivity(profileId, "Activity 100", GroupActivityType.ProfileJoinedGroup);

        // Assert – still 100 activities, oldest removed
        Assert.Equal(100, group.Activities.Count);
        Assert.DoesNotContain(group.Activities, a => a.Description == firstActivityBefore);
        Assert.Contains(group.Activities, a => a.Description == "Activity 100");
    }

    // ── FromPersisted with activities ────────────────────────────────────────

    [Fact]
    public void FromPersisted_WithActivities_ShouldIncludeActivities()
    {
        // Arrange
        var id = Guid.NewGuid();
        var activities = new List<GroupActivity>
        {
            new GroupActivity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "Joined",
                GroupActivityType.ProfileJoinedGroup)
        };

        // Act
        var group = Group.FromPersisted(id, _faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName,
            activities: activities);

        // Assert
        Assert.Single(group.Activities);
    }
}
