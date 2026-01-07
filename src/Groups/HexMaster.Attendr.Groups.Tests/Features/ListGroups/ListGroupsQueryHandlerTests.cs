using Bogus;
using HexMaster.Attendr.Groups.Features.ListGroups;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.ListGroups;

public sealed class ListGroupsQueryHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<ListGroupsQueryHandler>> _mockLogger;
    private readonly ListGroupsQueryHandler _handler;
    private readonly Faker _faker;

    public ListGroupsQueryHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<ListGroupsQueryHandler>>();
        _handler = new ListGroupsQueryHandler(
            _mockRepository.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithGroups_ShouldReturnPaginatedGroups()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var groups = new List<HexMaster.Attendr.Groups.DomainModels.Group>
        {
            GroupFactory.CreatePersistedGroup(),
            GroupFactory.CreatePersistedGroup(),
            GroupFactory.CreatePersistedGroup()
        };
        var totalCount = 3;
        var query = new ListGroupsQuery(profileId, null, 10, 1);

        _mockRepository.Setup(x => x.ListGroupsAsync(null, 10, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((groups, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Groups.Count);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.PageNumber);
    }

    [Fact]
    public async Task Handle_WithSearchQuery_ShouldReturnFilteredGroups()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var searchQuery = "Tech";
        var groups = new List<HexMaster.Attendr.Groups.DomainModels.Group>
        {
            GroupFactory.CreatePersistedGroup(name: "Tech Enthusiasts")
        };
        var totalCount = 1;
        var query = new ListGroupsQuery(profileId, searchQuery, 10, 1);

        _mockRepository.Setup(x => x.ListGroupsAsync(searchQuery, 10, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((groups, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Groups);
        Assert.Equal(totalCount, result.TotalCount);
    }

    [Fact]
    public async Task Handle_WithMemberProfile_ShouldIndicateMembership()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profileName = _faker.Person.FullName;
        var group = GroupFactory.CreatePersistedGroup(ownerId: profileId, ownerName: profileName);
        var groups = new List<HexMaster.Attendr.Groups.DomainModels.Group> { group };
        var totalCount = 1;
        var query = new ListGroupsQuery(profileId, null, 10, 1);

        _mockRepository.Setup(x => x.ListGroupsAsync(null, 10, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((groups, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Groups);
        Assert.True(result.Groups.First().IsMember);
    }

    [Fact]
    public async Task Handle_WithNonMemberProfile_ShouldIndicateNonMembership()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        var groups = new List<HexMaster.Attendr.Groups.DomainModels.Group> { group };
        var totalCount = 1;
        var query = new ListGroupsQuery(profileId, null, 10, 1);

        _mockRepository.Setup(x => x.ListGroupsAsync(null, 10, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((groups, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Groups);
        Assert.False(result.Groups.First().IsMember);
    }

    [Fact]
    public async Task Handle_WhenNoGroups_ShouldReturnEmptyResult()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var groups = new List<HexMaster.Attendr.Groups.DomainModels.Group>();
        var totalCount = 0;
        var query = new ListGroupsQuery(profileId, null, 10, 1);

        _mockRepository.Setup(x => x.ListGroupsAsync(null, 10, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((groups, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Groups);
        Assert.Equal(0, result.TotalCount);
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
        var profileId = Guid.NewGuid();
        var query = new ListGroupsQuery(profileId, null, 10, 1);

        _mockRepository.Setup(x => x.ListGroupsAsync(null, 10, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
