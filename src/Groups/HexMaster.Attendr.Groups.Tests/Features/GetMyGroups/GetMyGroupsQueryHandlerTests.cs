using Bogus;
using HexMaster.Attendr.Groups.Features.GetMyGroups;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.GetMyGroups;

public class GetMyGroupsQueryHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepositoryMock;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<GetMyGroupsQueryHandler>> _loggerMock;
    private readonly GetMyGroupsQueryHandler _handler;
    private readonly Faker _faker = new();

    public GetMyGroupsQueryHandlerTests()
    {
        _groupRepositoryMock = new Mock<IGroupRepository>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _loggerMock = new Mock<ILogger<GetMyGroupsQueryHandler>>();
        _handler = new GetMyGroupsQueryHandler(
            _groupRepositoryMock.Object,
            _metrics,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenMemberOfMultipleGroups_ShouldReturnGroupsSortedAlphabetically()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var groupA = GroupFactory.CreatePersistedGroup(name: "Alpha Group");
        var groupB = GroupFactory.CreatePersistedGroup(name: "Zeta Group");
        var groupC = GroupFactory.CreatePersistedGroup(name: "Beta Group");

        var groups = new List<HexMaster.Attendr.Groups.DomainModels.Group> { groupB, groupC, groupA };
        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

        var query = new GetMyGroupsQuery(profileId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        var names = result.Select(g => g.Name).ToList();
        Assert.Equal("Alpha Group", names[0]);
        Assert.Equal("Beta Group", names[1]);
        Assert.Equal("Zeta Group", names[2]);
    }

    [Fact]
    public async Task Handle_WhenMemberOfNoGroups_ShouldReturnEmptyList()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group>());

        var query = new GetMyGroupsQuery(profileId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenMemberOfOneGroup_ShouldReturnMappedDto()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup(name: "Test Group");

        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group });

        var query = new GetMyGroupsQuery(profileId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.Single(result);
        var dto = result.First();
        Assert.Equal(group.Id, dto.Id);
        Assert.Equal("Test Group", dto.Name);
    }

    [Fact]
    public async Task Handle_WhenQueryIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!));
    }
}
