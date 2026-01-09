using Bogus;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

public sealed class GroupTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Create_WithValidData_ShouldCreateGroup()
    {
        // Arrange
        var name = _faker.Company.CompanyName();
        var ownerId = Guid.NewGuid();
        var ownerName = _faker.Person.FullName;

        // Act
        var group = Group.Create(name, ownerId, ownerName);

        // Assert
        Assert.NotEqual(Guid.Empty, group.Id);
        Assert.Equal(name, group.Name);
        Assert.NotNull(group.Settings);
        Assert.Single(group.Members);
        Assert.Equal(ownerId, group.Members.First().Id);
        Assert.Equal(GroupRole.Owner, group.Members.First().Role);
    }

    [Fact]
    public void Create_WithPublicSettings_ShouldCreatePublicGroup()
    {
        // Arrange
        var name = _faker.Company.CompanyName();
        var ownerId = Guid.NewGuid();
        var ownerName = _faker.Person.FullName;

        // Act
        var group = Group.Create(name, ownerId, ownerName, isPublic: true);

        // Assert
        Assert.True(group.Settings.IsPublic);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var ownerName = _faker.Person.FullName;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Group.Create(null!, ownerId, ownerName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var ownerName = _faker.Person.FullName;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Group.Create(invalidName, ownerId, ownerName));
    }

    [Fact]
    public void FromPersisted_WithValidData_ShouldCreateGroup()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = _faker.Company.CompanyName();
        var ownerId = Guid.NewGuid();
        var ownerName = _faker.Person.FullName;

        // Act
        var group = Group.FromPersisted(id, name, ownerId, ownerName);

        // Assert
        Assert.Equal(id, group.Id);
        Assert.Equal(name, group.Name);
        Assert.Single(group.Members);
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var newName = "Updated Group Name";

        // Act
        group.UpdateName(newName);

        // Assert
        Assert.Equal(newName, group.Name);
    }

    [Fact]
    public void UpdateName_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => group.UpdateName(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithEmptyName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => group.UpdateName(invalidName));
    }

    [Fact]
    public void UpdateSettings_WithValidSettings_ShouldUpdateSettings()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var newSettings = GroupSettings.Create(isPublic: true, isSearchable: true);

        // Act
        group.UpdateSettings(newSettings);

        // Assert
        Assert.True(group.Settings.IsPublic);
        Assert.True(group.Settings.IsSearchable);
    }

    [Fact]
    public void UpdateSettings_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => group.UpdateSettings(null!));
    }

    [Fact]
    public void AddMember_WithValidData_ShouldAddMember()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        var memberName = _faker.Person.FullName;

        // Act
        group.AddMember(memberId, memberName, GroupRole.Member);

        // Assert
        Assert.Equal(2, group.Members.Count);
        Assert.Contains(group.Members, m => m.Id == memberId);
    }

    [Fact]
    public void AddMember_WithOwnerRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        var memberName = _faker.Person.FullName;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.AddMember(memberId, memberName, GroupRole.Owner));
    }

    [Fact]
    public void AddMember_WithDuplicateMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        var memberName = _faker.Person.FullName;
        group.AddMember(memberId, memberName, GroupRole.Member);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.AddMember(memberId, memberName, GroupRole.Member));
    }

    [Fact]
    public void RemoveMember_WithValidMember_ShouldRemoveMember()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        var memberName = _faker.Person.FullName;
        group.AddMember(memberId, memberName, GroupRole.Member);

        // Act
        group.RemoveMember(memberId);

        // Assert
        Assert.Single(group.Members);
        Assert.DoesNotContain(group.Members, m => m.Id == memberId);
    }

    [Fact]
    public void RemoveMember_WithOwner_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = Group.Create(_faker.Company.CompanyName(), ownerId, _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => group.RemoveMember(ownerId));
    }

    [Fact]
    public void RemoveMember_WithNonExistentMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => group.RemoveMember(nonExistentId));
    }

    [Fact]
    public void UpdateMemberRole_WithValidData_ShouldUpdateRole()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        var memberName = _faker.Person.FullName;
        group.AddMember(memberId, memberName, GroupRole.Member);

        // Act
        group.UpdateMemberRole(memberId, GroupRole.Manager);

        // Assert
        var member = group.Members.First(m => m.Id == memberId);
        Assert.Equal(GroupRole.Manager, member.Role);
    }

    [Fact]
    public void UpdateMemberRole_ToOwner_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        var memberName = _faker.Person.FullName;
        group.AddMember(memberId, memberName, GroupRole.Member);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.UpdateMemberRole(memberId, GroupRole.Owner));
    }

    [Fact]
    public void UpdateMemberRole_OfOwner_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = Group.Create(_faker.Company.CompanyName(), ownerId, _faker.Person.FullName);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.UpdateMemberRole(ownerId, GroupRole.Manager));
    }

    [Fact]
    public void TransferOwnership_WithValidMember_ShouldTransferOwnership()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = Group.Create(_faker.Company.CompanyName(), ownerId, _faker.Person.FullName);
        var newOwnerId = Guid.NewGuid();
        group.AddMember(newOwnerId, _faker.Person.FullName, GroupRole.Manager);

        // Act
        group.TransferOwnership(newOwnerId);

        // Assert
        var oldOwner = group.Members.First(m => m.Id == ownerId);
        var newOwner = group.Members.First(m => m.Id == newOwnerId);
        Assert.Equal(GroupRole.Manager, oldOwner.Role);
        Assert.Equal(GroupRole.Owner, newOwner.Role);
    }

    [Fact]
    public void TransferOwnership_ToNonMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var nonMemberId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => group.TransferOwnership(nonMemberId));
    }

    [Fact]
    public void AddInvitation_WithValidData_ShouldAddInvitation()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var inviteeId = Guid.NewGuid();
        var inviteeName = _faker.Person.FullName;
        var expirationDate = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        group.AddInvitation(inviteeId, inviteeName, expirationDate);

        // Assert
        Assert.Single(group.Invitations);
        Assert.Contains(group.Invitations, i => i.Id == inviteeId);
    }

    [Fact]
    public void AddInvitation_ToExistingMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        var memberName = _faker.Person.FullName;
        group.AddMember(memberId, memberName, GroupRole.Member);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            group.AddInvitation(memberId, memberName, DateTimeOffset.UtcNow.AddDays(7)));
    }

    [Fact]
    public void RemoveInvitation_WithValidInvitation_ShouldRemoveInvitation()
    {
        // Arrange
        var group = Group.Create(_faker.Company.CompanyName(), Guid.NewGuid(), _faker.Person.FullName);
        var inviteeId = Guid.NewGuid();
        group.AddInvitation(inviteeId, _faker.Person.FullName, DateTimeOffset.UtcNow.AddDays(7));

        // Act
        group.RemoveInvitation(inviteeId);

        // Assert
        Assert.Empty(group.Invitations);
    }
}
