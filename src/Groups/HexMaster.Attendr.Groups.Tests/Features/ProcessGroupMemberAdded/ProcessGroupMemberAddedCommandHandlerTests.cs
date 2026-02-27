using Bogus;
using HexMaster.Attendr.Groups.Features.ProcessGroupMemberAdded;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.IntegrationEvents.Events.Groups;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.ProcessGroupMemberAdded;

public class ProcessGroupMemberAddedCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepositoryMock;
    private readonly Mock<ILogger<ProcessGroupMemberAddedCommandHandler>> _loggerMock;
    private readonly ProcessGroupMemberAddedCommandHandler _handler;
    private readonly Faker _faker = new();

    public ProcessGroupMemberAddedCommandHandlerTests()
    {
        _groupRepositoryMock = new Mock<IGroupRepository>();
        _loggerMock = new Mock<ILogger<ProcessGroupMemberAddedCommandHandler>>();
        _handler = new ProcessGroupMemberAddedCommandHandler(
            _groupRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenGroupExists_ShouldAddActivityAndPersist()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        var @event = new GroupMemberAddedEvent
        {
            GroupId = group.Id,
            GroupName = group.Name,
            ProfileId = profileId,
            Role = "Member"
        };

        _groupRepositoryMock
            .Setup(r => r.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var command = new ProcessGroupMemberAddedCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert
        _groupRepositoryMock.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldReturnSilently()
    {
        // Arrange
        var @event = new GroupMemberAddedEvent
        {
            GroupId = Guid.NewGuid(),
            GroupName = "Some Group",
            ProfileId = Guid.NewGuid(),
            Role = "Member"
        };

        _groupRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HexMaster.Attendr.Groups.DomainModels.Group?)null);

        var command = new ProcessGroupMemberAddedCommand(@event);

        // Act (should not throw)
        await _handler.Handle(command);

        // Assert - update should not be called
        _groupRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<HexMaster.Attendr.Groups.DomainModels.Group>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCommandIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!));
    }
}
