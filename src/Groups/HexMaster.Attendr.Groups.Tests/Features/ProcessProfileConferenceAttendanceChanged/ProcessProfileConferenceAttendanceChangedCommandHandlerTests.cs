using Bogus;
using HexMaster.Attendr.Groups.Features.ProcessProfileConferenceAttendanceChanged;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.ProcessProfileConferenceAttendanceChanged;

public class ProcessProfileConferenceAttendanceChangedCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepositoryMock;
    private readonly Mock<ILogger<ProcessProfileConferenceAttendanceChangedCommandHandler>> _loggerMock;
    private readonly ProcessProfileConferenceAttendanceChangedCommandHandler _handler;
    private readonly Faker _faker = new();

    public ProcessProfileConferenceAttendanceChangedCommandHandlerTests()
    {
        _groupRepositoryMock = new Mock<IGroupRepository>();
        _loggerMock = new Mock<ILogger<ProcessProfileConferenceAttendanceChangedCommandHandler>>();
        _handler = new ProcessProfileConferenceAttendanceChangedCommandHandler(
            _groupRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProfileIsAttending_ShouldAddAttendingActivityToEachGroup()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group });

        var @event = new ProfileConferenceAttendanceChangedEvent
        {
            ProfileId = profileId,
            ConferenceId = Guid.NewGuid(),
            ConferenceName = "DevConf 2025",
            IsAttending = true
        };
        var command = new ProcessProfileConferenceAttendanceChangedCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert
        _groupRepositoryMock.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProfileIsNotAttending_ShouldAddLeavingActivityToEachGroup()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group });

        var @event = new ProfileConferenceAttendanceChangedEvent
        {
            ProfileId = profileId,
            ConferenceId = Guid.NewGuid(),
            ConferenceName = "DevConf 2025",
            IsAttending = false
        };
        var command = new ProcessProfileConferenceAttendanceChangedCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert
        _groupRepositoryMock.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProfileIsNotMemberOfAnyGroup_ShouldReturnWithoutUpdating()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group>());

        var @event = new ProfileConferenceAttendanceChangedEvent
        {
            ProfileId = profileId,
            ConferenceId = Guid.NewGuid(),
            ConferenceName = "DevConf 2025",
            IsAttending = true
        };
        var command = new ProcessProfileConferenceAttendanceChangedCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert - update never called if no groups
        _groupRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<HexMaster.Attendr.Groups.DomainModels.Group>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProfileIsInMultipleGroups_ShouldUpdateAllGroups()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group1 = GroupFactory.CreatePersistedGroup();
        var group2 = GroupFactory.CreatePersistedGroup();
        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group1, group2 });

        var @event = new ProfileConferenceAttendanceChangedEvent
        {
            ProfileId = profileId,
            ConferenceId = Guid.NewGuid(),
            ConferenceName = "Conference X",
            IsAttending = true
        };
        var command = new ProcessProfileConferenceAttendanceChangedCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert
        _groupRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<HexMaster.Attendr.Groups.DomainModels.Group>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenCommandIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!));
    }
}
