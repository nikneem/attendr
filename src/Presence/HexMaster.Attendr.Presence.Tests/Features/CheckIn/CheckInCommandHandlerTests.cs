using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.CheckIn;
using HexMaster.Attendr.Presence.Tests.Factories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.CheckIn;

public sealed class CheckInCommandHandlerTests
{
    private readonly Mock<IPresentationPresenceRepository> _repositoryMock;
    private readonly Mock<IIntegrationEventPublisher> _publisherMock;
    private readonly CheckInCommandHandler _sut;

    public CheckInCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPresentationPresenceRepository>();
        _publisherMock = new Mock<IIntegrationEventPublisher>();
        _sut = new CheckInCommandHandler(
            _repositoryMock.Object,
            _publisherMock.Object,
            NullLogger<CheckInCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenCheckingIn_ShouldSetIsCheckedInAndPublishEvent()
    {
        var presentation = PresentationPresenceFactory.Create(isCheckedIn: false);
        var command = new CheckInCommand(
            presentation.ProfileId,
            presentation.ConferenceId,
            presentation.PresentationId,
            true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId, default))
            .ReturnsAsync(presentation);

        await _sut.Handle(command);

        Assert.True(presentation.IsCheckedIn);
        _repositoryMock.Verify(r => r.UpdateAsync(
            presentation.ProfileId, presentation.ConferenceId, presentation, default), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<HexMaster.Attendr.IntegrationEvents.Events.Profiles.ProfileCheckedInEvent>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCheckingOut_ShouldClearIsCheckedIn()
    {
        var presentation = PresentationPresenceFactory.Create(isCheckedIn: true, checkedInAt: DateTimeOffset.UtcNow);
        var command = new CheckInCommand(
            presentation.ProfileId,
            presentation.ConferenceId,
            presentation.PresentationId,
            false);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId, default))
            .ReturnsAsync(presentation);

        await _sut.Handle(command);

        Assert.False(presentation.IsCheckedIn);
        _repositoryMock.Verify(r => r.UpdateAsync(
            presentation.ProfileId, presentation.ConferenceId, presentation, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPresentationNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new CheckInCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((PresentationPresence?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.Handle(command));
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Handle(null!));
    }
}
