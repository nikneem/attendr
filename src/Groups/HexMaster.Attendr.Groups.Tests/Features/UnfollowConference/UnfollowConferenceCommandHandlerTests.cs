using Bogus;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.UnfollowConference;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.UnfollowConference;

public sealed class UnfollowConferenceCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<UnfollowConferenceCommandHandler>> _mockLogger;
    private readonly UnfollowConferenceCommandHandler _handler;
    private readonly Faker _faker;

    public UnfollowConferenceCommandHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<UnfollowConferenceCommandHandler>>();
        _handler = new UnfollowConferenceCommandHandler(
            _mockRepository.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WhenMemberUnfollowsConference_ShouldRemoveConference()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var conferenceId = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(12));
        group.FollowConference(conferenceId, "TechConf", "Amsterdam", "NL", null, 5, 10, start, end);

        var command = new UnfollowConferenceCommand(group.Id, conferenceId, ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Empty(group.FollowedConferences);
        _mockRepository.Verify(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowInvalidOperationException()
    {
        var command = new UnfollowConferenceCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberNotInGroup_ShouldThrowInvalidOperationException()
    {
        var group = GroupFactory.CreateGroup();
        var command = new UnfollowConferenceCommand(group.Id, Guid.NewGuid(), Guid.NewGuid());

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!, CancellationToken.None));
    }
}
