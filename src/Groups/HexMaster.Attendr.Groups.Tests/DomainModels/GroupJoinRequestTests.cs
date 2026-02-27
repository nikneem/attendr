using Bogus;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

public sealed class GroupJoinRequestTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = _faker.Person.FullName;
        var requestedAt = DateTimeOffset.UtcNow;

        // Act
        var request = new GroupJoinRequest(id, name, requestedAt);

        // Assert
        Assert.Equal(id, request.Id);
        Assert.Equal(name, request.Name);
        Assert.Equal(requestedAt, request.RequestedAt);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new GroupJoinRequest(Guid.Empty, _faker.Person.FullName, DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new GroupJoinRequest(Guid.NewGuid(), invalidName!, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void RequestDate_InterfaceProperty_ShouldMatchRequestedAt()
    {
        // Arrange
        var requestedAt = DateTimeOffset.UtcNow;
        GroupJoinRequest request = new(Guid.NewGuid(), _faker.Person.FullName, requestedAt);

        // Act – access through interface
        HexMaster.Attendr.Groups.Abstractions.DomainModels.IGroupJoinRequest iRequest = request;

        // Assert
        Assert.Equal(requestedAt, iRequest.RequestDate);
    }
}
