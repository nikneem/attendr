using Bogus;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

public sealed class GroupActivityTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateActivity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        const string description = "Profile joined the group";
        var activityType = GroupActivityType.ProfileJoinedGroup;

        // Act
        var activity = new GroupActivity(id, profileId, createdAt, description, activityType);

        // Assert
        Assert.Equal(id, activity.Id);
        Assert.Equal(profileId, activity.ProfileId);
        Assert.Equal(createdAt, activity.CreatedAt);
        Assert.Equal(description, activity.Description);
        Assert.Equal(activityType, activity.ActivityType);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new GroupActivity(Guid.Empty, Guid.NewGuid(), DateTimeOffset.UtcNow, "desc", GroupActivityType.ProfileJoinedGroup));
    }

    [Fact]
    public void Constructor_WithEmptyProfileId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new GroupActivity(Guid.NewGuid(), Guid.Empty, DateTimeOffset.UtcNow, "desc", GroupActivityType.ProfileJoinedGroup));
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GroupActivity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, null!, GroupActivityType.ProfileJoinedGroup));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyDescription_ShouldThrowArgumentException(string? invalidDesc)
    {
        Assert.Throws<ArgumentException>(() =>
            new GroupActivity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, invalidDesc!, GroupActivityType.ProfileJoinedGroup));
    }

    [Fact]
    public void Constructor_WithNullActivityType_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GroupActivity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "desc", null!));
    }

    [Fact]
    public void Description_ShouldBeTrimmed()
    {
        var activity = new GroupActivity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow,
            "  some description  ", GroupActivityType.ProfileJoinedGroup);

        Assert.Equal("some description", activity.Description);
    }

    [Fact]
    public void ActivityDate_InterfaceProperty_ShouldMatchCreatedAt()
    {
        var createdAt = DateTimeOffset.UtcNow;
        GroupActivity activity = new(Guid.NewGuid(), Guid.NewGuid(), createdAt, "desc", GroupActivityType.ProfileJoinedGroup);

        IGroupActivity iActivity = activity;

        Assert.Equal(createdAt, iActivity.ActivityDate);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void GroupActivityType_FromId_ShouldReturnCorrectType(int activityTypeId)
    {
        var activityType = GroupActivityType.FromId(activityTypeId);

        Assert.Equal(activityTypeId, activityType.ActivityTypeId);
    }

    [Fact]
    public void GroupActivityType_FromId_WithInvalidId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => GroupActivityType.FromId(999));
    }

    [Fact]
    public void AllGroupActivityTypes_ShouldHaveCorrectProperties()
    {
        Assert.Equal(1, GroupActivityType.ProfileJoinedGroup.ActivityTypeId);
        Assert.Equal(ActivitySeverity.Medium, GroupActivityType.ProfileJoinedGroup.Severity);
        Assert.NotEmpty(GroupActivityType.ProfileJoinedGroup.TranslationKey);

        Assert.Equal(2, GroupActivityType.ProfileLeftGroup.ActivityTypeId);
        Assert.Equal(ActivitySeverity.Low, GroupActivityType.ProfileLeftGroup.Severity);

        Assert.Equal(3, GroupActivityType.ProfilePresentationCheckedIn.ActivityTypeId);
        Assert.Equal(ActivitySeverity.High, GroupActivityType.ProfilePresentationCheckedIn.Severity);

        Assert.Equal(4, GroupActivityType.ProfilePresentationCheckedOut.ActivityTypeId);
        Assert.Equal(ActivitySeverity.Low, GroupActivityType.ProfilePresentationCheckedOut.Severity);

        Assert.Equal(5, GroupActivityType.ProfileAttendingConference.ActivityTypeId);
        Assert.Equal(ActivitySeverity.High, GroupActivityType.ProfileAttendingConference.Severity);

        Assert.Equal(6, GroupActivityType.ProfileLeavingConference.ActivityTypeId);
        Assert.Equal(ActivitySeverity.Low, GroupActivityType.ProfileLeavingConference.Severity);
    }
}
