using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.GetConferenceWithPresentations;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.GetConferenceWithPresentations;

public sealed class GetConferenceWithPresentationsQueryHandlerTests
{
    private readonly Mock<IConferencePresenceRepository> _confRepositoryMock;
    private readonly Mock<IPresentationPresenceRepository> _presRepositoryMock;
    private readonly GetConferenceWithPresentationsQueryHandler _sut;

    public GetConferenceWithPresentationsQueryHandlerTests()
    {
        _confRepositoryMock = new Mock<IConferencePresenceRepository>();
        _presRepositoryMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new GetConferenceWithPresentationsQueryHandler(
            _confRepositoryMock.Object,
            _presRepositoryMock.Object,
            metrics,
            NullLogger<GetConferenceWithPresentationsQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenConferencePresenceNotFound_ShouldThrowInvalidOperationException()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        _confRepositoryMock
            .Setup(r => r.GetAsync(conferenceId, profileId, default))
            .ReturnsAsync((ConferencePresence?)null);

        var query = new GetConferenceWithPresentationsQuery(profileId, conferenceId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(query));
    }

    [Fact]
    public async Task Handle_WhenConferenceFound_ShouldReturnResponseWithPresentations()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        var conference = ConferencePresenceFactory.Create(
            conferenceId: conferenceId,
            profileId: profileId,
            isFollowing: true,
            isAttending: true);

        var presentations = PresentationPresenceFactory.CreateList(3, profileId: profileId, conferenceId: conferenceId);

        _confRepositoryMock
            .Setup(r => r.GetAsync(conferenceId, profileId, default))
            .ReturnsAsync(conference);
        _presRepositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(presentations.AsReadOnly());

        var query = new GetConferenceWithPresentationsQuery(profileId, conferenceId);
        var result = await _sut.Handle(query);

        Assert.NotNull(result);
        Assert.Equal(conferenceId, result.ConferenceId);
        Assert.Equal(3, result.Presentations.Count);
    }

    [Fact]
    public async Task Handle_WhenNoPresentation_ShouldReturnEmptyPresentationsList()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        var conference = ConferencePresenceFactory.Create(
            conferenceId: conferenceId,
            profileId: profileId);

        _confRepositoryMock
            .Setup(r => r.GetAsync(conferenceId, profileId, default))
            .ReturnsAsync(conference);
        _presRepositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence>().AsReadOnly());

        var query = new GetConferenceWithPresentationsQuery(profileId, conferenceId);
        var result = await _sut.Handle(query);

        Assert.NotNull(result);
        Assert.Empty(result.Presentations);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        _confRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var query = new GetConferenceWithPresentationsQuery(profileId, conferenceId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(query));
    }
}
