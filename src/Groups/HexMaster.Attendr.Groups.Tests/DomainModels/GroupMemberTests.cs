using Bogus;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

public sealed class GroupMemberTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateGroupMember()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = _faker.Person.FullName;
        var role = GroupRole.Member;

        // Act
        var member = new GroupMember(id, name, role);

        // Assert
        Assert.Equal(id, member.Id);
        Assert.Equal(name, member.Name);
        Assert.Equal(role, member.Role);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var name = _faker.Person.FullName;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new GroupMember(Guid.Empty, name, GroupRole.Member));
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new GroupMember(id, null!, GroupRole.Member));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new GroupMember(id, invalidName, GroupRole.Member));
    }

    [Fact]
    public void UpdateRole_WithValidRole_ShouldUpdateRole()
    {
        // Arrange
        var member = new GroupMember(Guid.NewGuid(), _faker.Person.FullName, GroupRole.Member);

        // Act
        member.UpdateRole(GroupRole.Manager);

        // Assert
        Assert.Equal(GroupRole.Manager, member.Role);
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var member = new GroupMember(Guid.NewGuid(), _faker.Person.FullName, GroupRole.Member);
        var newName = "Updated Name";

        // Act
        member.UpdateName(newName);

        // Assert
        Assert.Equal(newName, member.Name);
    }

    [Fact]
    public void UpdateName_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var member = new GroupMember(Guid.NewGuid(), _faker.Person.FullName, GroupRole.Member);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => member.UpdateName(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithEmptyName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var member = new GroupMember(Guid.NewGuid(), _faker.Person.FullName, GroupRole.Member);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => member.UpdateName(invalidName));
    }
}
