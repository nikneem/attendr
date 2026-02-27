using Bogus;
using HexMaster.Attendr.Groups.Features.GetGroupFollowedConferences;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.GetGroupFollowedConferences;

public class GetGroupFollowedConferencesQueryHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepositoryMock;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<GetGroupFollowedConferencesQueryHandler>> _loggerMock;
    private readonly GetGroupFollowedConferencesQueryHandler _handler;
    private readonly Faker _faker = new();

    public GetGroupFollowedConferencesQueryHandlerTests()
    {
        _groupRepositoryMock = new Mock<IGroupRepository>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _loggerMock = new Mock<ILogger<GetGroupFollowedConferencesQueryHandler>>();
        _handler = new GetGroupFollowedConferencesQueryHandler(
            _groupRepositoryMock.Object,
            _metrics,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenMemberRequestsConferences_ShouldReturnFollowedConferences()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: "Owner");
        var futureStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var futureEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(13));
        group.FollowConference(Guid.NewGuid(), "Future Conf", "London", "UK", null, 10, 20, futureStart, futureEnd);

        _groupRepositoryMock
            .Setup(r => r.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var query = new GetGroupFollowedConferencesQuery(group.Id, ownerId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Future Conf", result.First().Name);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _groupRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HexMaster.Attendr.Groups.DomainModels.Group?)null);

        var query = new GetGroupFollowedConferencesQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query));
    }

    [Fact]
    public async Task Handle_WhenRequestingProfileIsNotMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = GroupFactory.CreateGroup(ownerId: Guid.NewGuid());
        _groupRepositoryMock
            .Setup(r => r.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var nonMemberId = Guid.NewGuid();
        var query = new GetGroupFollowedConferencesQuery(group.Id, nonMemberId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query));
    }

    [Fact]
    public async Task Handle_WhenNoFutureConferences_ShouldReturnEmptyList()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: "Owner");
        // Add a past conference
        var pastStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var pastEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        group.FollowConference(Guid.NewGuid(), "Past Conf", "Berlin", "Germany", null, 5, 15, pastStart, pastEnd);

        _groupRepositoryMock
            .Setup(r => r.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var query = new GetGroupFollowedConferencesQuery(group.Id, ownerId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenQueryIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!));
    }
}
