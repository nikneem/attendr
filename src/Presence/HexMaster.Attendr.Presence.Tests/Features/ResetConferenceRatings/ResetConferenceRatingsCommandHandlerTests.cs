using HexMaster.Attendr.Presence.Features.ResetConferenceRatings;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.ResetConferenceRatings;

public sealed class ResetConferenceRatingsCommandHandlerTests
{
    private readonly Mock<IPresentationPresenceRepository> _repositoryMock;
    private readonly ResetConferenceRatingsCommandHandler _sut;

    public ResetConferenceRatingsCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new ResetConferenceRatingsCommandHandler(
            _repositoryMock.Object,
            metrics,
            NullLogger<ResetConferenceRatingsCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ShouldCallResetRatingsAsync()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var command = new ResetConferenceRatingsCommand(profileId, conferenceId);

        _repositoryMock
            .Setup(r => r.ResetRatingsAsync(profileId, conferenceId, default))
            .ReturnsAsync(5);

        await _sut.Handle(command);

        _repositoryMock.Verify(r =>
            r.ResetRatingsAsync(profileId, conferenceId, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenResetThrows_ShouldPropagateException()
    {
        _repositoryMock
            .Setup(r => r.ResetRatingsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var command = new ResetConferenceRatingsCommand(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenNoPresentationsAffected_ShouldSucceed()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var command = new ResetConferenceRatingsCommand(profileId, conferenceId);

        _repositoryMock
            .Setup(r => r.ResetRatingsAsync(profileId, conferenceId, default))
            .ReturnsAsync(0);

        // Should complete without error
        await _sut.Handle(command);
    }
}
