using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.UpdateAttendance;
using HexMaster.Attendr.Presence.Tests.Factories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandlerTests
{
    private readonly Mock<IConferencePresenceRepository> _repositoryMock;
    private readonly Mock<IIntegrationEventPublisher> _publisherMock;
    private readonly UpdateAttendanceCommandHandler _sut;

    public UpdateAttendanceCommandHandlerTests()
    {
        _repositoryMock = new Mock<IConferencePresenceRepository>();
        _publisherMock = new Mock<IIntegrationEventPublisher>();
        _sut = new UpdateAttendanceCommandHandler(
            _repositoryMock.Object,
            _publisherMock.Object,
            NullLogger<UpdateAttendanceCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenPresenceFound_ShouldUpdateAndPublishEvent()
    {
        var presence = ConferencePresenceFactory.Create(isAttending: false);
        var command = new UpdateAttendanceCommand(presence.ConferenceId, presence.ProfileId, true);

        _repositoryMock
            .Setup(r => r.GetAsync(presence.ConferenceId, presence.ProfileId, default))
            .ReturnsAsync(presence);

        await _sut.Handle(command, CancellationToken.None);

        Assert.True(presence.IsAttending);
        _repositoryMock.Verify(r => r.UpdateAsync(presence, default), Times.Once);
        _publisherMock.Verify(p =>
            p.PublishAsync(It.IsAny<HexMaster.Attendr.IntegrationEvents.Events.Profiles.ProfileConferenceAttendanceChangedEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPresenceNotFound_ShouldThrowInvalidOperationException()
    {
        _repositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((ConferencePresence?)null);

        var command = new UpdateAttendanceCommand(Guid.NewGuid(), Guid.NewGuid(), true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SetNotAttending_ShouldUpdateIsAttendingFalse()
    {
        var presence = ConferencePresenceFactory.Create(isAttending: true);
        var command = new UpdateAttendanceCommand(presence.ConferenceId, presence.ProfileId, false);

        _repositoryMock
            .Setup(r => r.GetAsync(presence.ConferenceId, presence.ProfileId, default))
            .ReturnsAsync(presence);

        await _sut.Handle(command, CancellationToken.None);

        Assert.False(presence.IsAttending);
    }
}
