using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.UnfollowConference;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.UnfollowConference;

public sealed class UnfollowConferenceCommandHandlerTests
{
    private readonly Mock<IConferencePresenceRepository> _conferenceRepositoryMock;
    private readonly Mock<IPresentationPresenceRepository> _presentationRepositoryMock;
    private readonly UnfollowConferenceCommandHandler _sut;

    public UnfollowConferenceCommandHandlerTests()
    {
        _conferenceRepositoryMock = new Mock<IConferencePresenceRepository>();
        _presentationRepositoryMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new UnfollowConferenceCommandHandler(
            _conferenceRepositoryMock.Object,
            _presentationRepositoryMock.Object,
            metrics,
            NullLogger<UnfollowConferenceCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenFollowing_ShouldDeletePresentationsAndConference()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var command = new UnfollowConferenceCommand(conferenceId, profileId);

        var presentations = PresentationPresenceFactory.CreateList(2, profileId: profileId, conferenceId: conferenceId);

        _conferenceRepositoryMock
            .Setup(r => r.ExistsAsync(profileId, conferenceId, default))
            .ReturnsAsync(true);
        _presentationRepositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(presentations.AsReadOnly());

        await _sut.Handle(command);

        _presentationRepositoryMock.Verify(r =>
            r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default),
            Times.Exactly(2));
        _conferenceRepositoryMock.Verify(r =>
            r.DeleteAsync(conferenceId, profileId, default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFollowing_ShouldThrowInvalidOperationException()
    {
        _conferenceRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(false);

        var command = new UnfollowConferenceCommand(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenNoPresentations_ShouldDeleteConferenceOnly()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var command = new UnfollowConferenceCommand(conferenceId, profileId);

        _conferenceRepositoryMock
            .Setup(r => r.ExistsAsync(profileId, conferenceId, default))
            .ReturnsAsync(true);
        _presentationRepositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence>().AsReadOnly());

        await _sut.Handle(command);

        _presentationRepositoryMock.Verify(r =>
            r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default),
            Times.Never);
        _conferenceRepositoryMock.Verify(r =>
            r.DeleteAsync(conferenceId, profileId, default),
            Times.Once);
    }
}
