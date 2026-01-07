using Bogus;
using HexMaster.Attendr.Groups.Features.GetGroupDetails;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.GetGroupDetails;

public sealed class GetGroupDetailsQueryHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<GetGroupDetailsQueryHandler>> _mockLogger;
    private readonly GetGroupDetailsQueryHandler _handler;
    private readonly Faker _faker;

    public GetGroupDetailsQueryHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<GetGroupDetailsQueryHandler>>();
        _handler = new GetGroupDetailsQueryHandler(
            _mockRepository.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WhenGroupExists_ShouldReturnGroupDetails()
    {
        // Arrange
        var group = GroupFactory.CreatePersistedGroup();
        var profileId = Guid.NewGuid();
        var query = new GetGroupDetailsQuery(group.Id, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(group.Id, result.Id);
        Assert.Equal(group.Name, result.Name);
        Assert.Equal(group.Members.Count, result.MemberCount);
        _mockRepository.Verify(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldReturnNull()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var query = new GetGroupDetailsQuery(groupId, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HexMaster.Attendr.Groups.DomainModels.Group?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProfileIsMember_ShouldIndicateMembership()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup(ownerId: profileId, ownerName: _faker.Person.FullName);
        var query = new GetGroupDetailsQuery(group.Id, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsMember);
        Assert.NotNull(result.CurrentMemberRole);
    }

    [Fact]
    public async Task Handle_WhenProfileIsNotMember_ShouldIndicateNonMembership()
    {
        // Arrange
        var group = GroupFactory.CreatePersistedGroup();
        var profileId = Guid.NewGuid();
        var query = new GetGroupDetailsQuery(group.Id, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsMember);
        Assert.Null(result.CurrentMemberRole);
    }

    [Fact]
    public async Task Handle_WithNullQuery_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var query = new GetGroupDetailsQuery(groupId, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
