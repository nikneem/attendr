using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.SetPreferredPresentation;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.SetPreferredPresentation;

public sealed class SetPreferredPresentationCommandHandlerTests
{
    private readonly Mock<IPresentationPresenceRepository> _repositoryMock;
    private readonly SetPreferredPresentationCommandHandler _sut;

    public SetPreferredPresentationCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new SetPreferredPresentationCommandHandler(
            _repositoryMock.Object,
            metrics,
            NullLogger<SetPreferredPresentationCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenPresentationNotFound_ShouldThrowKeyNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((PresentationPresence?)null);

        var command = new SetPreferredPresentationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenPresentationIsNotFavorite_ShouldThrowInvalidOperationException()
    {
        var presentation = PresentationPresenceFactory.Create(isFavorite: false);
        var command = new SetPreferredPresentationCommand(
            presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId, default))
            .ReturnsAsync(presentation);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenFavoritePresentationExists_ShouldSetAsPreferred()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(1);

        var target = PresentationPresenceFactory.Create(
            profileId: profileId, conferenceId: conferenceId,
            isFavorite: true, startDateTime: start, endDateTime: end);

        var command = new SetPreferredPresentationCommand(profileId, conferenceId, target.PresentationId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(profileId, conferenceId, target.PresentationId, default))
            .ReturnsAsync(target);
        _repositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { target }.AsReadOnly());

        await _sut.Handle(command);

        Assert.True(target.IsPreferred);
        _repositoryMock.Verify(r =>
            r.UpdateAsync(profileId, conferenceId, target, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOverlappingFavorites_ShouldUnsetOverlappingAndSetTarget()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(1);

        // target and overlapping share the same timeslot
        var target = PresentationPresenceFactory.Create(
            profileId: profileId, conferenceId: conferenceId,
            isFavorite: true, isPreferred: false, startDateTime: start, endDateTime: end);
        var overlapping = PresentationPresenceFactory.Create(
            profileId: profileId, conferenceId: conferenceId,
            isFavorite: true, isPreferred: true,
            startDateTime: start.AddMinutes(15), endDateTime: end.AddMinutes(15));

        var command = new SetPreferredPresentationCommand(profileId, conferenceId, target.PresentationId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(profileId, conferenceId, target.PresentationId, default))
            .ReturnsAsync(target);
        _repositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { target, overlapping }.AsReadOnly());

        await _sut.Handle(command);

        Assert.True(target.IsPreferred);
        Assert.False(overlapping.IsPreferred);
        // Called once for unset overlapping + once for set target
        _repositoryMock.Verify(r =>
            r.UpdateAsync(profileId, conferenceId, It.IsAny<PresentationPresence>(), default),
            Times.Exactly(2));
    }
}
