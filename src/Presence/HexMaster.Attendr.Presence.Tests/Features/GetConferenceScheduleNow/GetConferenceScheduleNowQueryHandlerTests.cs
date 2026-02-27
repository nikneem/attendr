using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.GetConferenceScheduleNow;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.GetConferenceScheduleNow;

public sealed class GetConferenceScheduleNowQueryHandlerTests
{
    private readonly Mock<IPresentationPresenceRepository> _repositoryMock;
    private readonly GetConferenceScheduleNowQueryHandler _sut;

    public GetConferenceScheduleNowQueryHandlerTests()
    {
        _repositoryMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new GetConferenceScheduleNowQueryHandler(
            _repositoryMock.Object,
            metrics,
            NullLogger<GetConferenceScheduleNowQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenNoPresentations_ShouldReturnEmptyResponse()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence>().AsReadOnly());

        var query = new GetConferenceScheduleNowQuery(profileId, conferenceId);
        var result = await _sut.Handle(query);

        Assert.Empty(result.Previous);
        Assert.Empty(result.Now);
        Assert.Empty(result.Next);
    }

    [Fact]
    public async Task Handle_WhenPresentationsExistButNoneAreFavorites_ShouldReturnEmptyResponse()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var presentations = new List<PresentationPresence>
        {
            PresentationPresenceFactory.Create(
                profileId: profileId,
                conferenceId: conferenceId,
                startDateTime: now.AddHours(-1),
                endDateTime: now.AddHours(1),
                isFavorite: false)
        };

        _repositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(presentations.AsReadOnly());

        var query = new GetConferenceScheduleNowQuery(profileId, conferenceId);
        var result = await _sut.Handle(query);

        Assert.Empty(result.Previous);
        Assert.Empty(result.Now);
        Assert.Empty(result.Next);
    }

    [Fact]
    public async Task Handle_WhenFavoritePresentationIsRunningNow_ShouldReturnInNow()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var currentPresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            conferenceId: conferenceId,
            startDateTime: now.AddMinutes(-30),
            endDateTime: now.AddMinutes(30),
            isFavorite: true);

        _repositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { currentPresentation }.AsReadOnly());

        var query = new GetConferenceScheduleNowQuery(profileId, conferenceId);
        var result = await _sut.Handle(query);

        Assert.Single(result.Now);
        Assert.Equal(currentPresentation.PresentationId, result.Now.First().PresentationId);
    }

    [Fact]
    public async Task Handle_WhenFavoritePresentationsInFutureOnly_ShouldReturnInNext()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var futurePresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            conferenceId: conferenceId,
            startDateTime: now.AddHours(2),
            endDateTime: now.AddHours(3),
            isFavorite: true);

        _repositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { futurePresentation }.AsReadOnly());

        var query = new GetConferenceScheduleNowQuery(profileId, conferenceId);
        var result = await _sut.Handle(query);

        Assert.Empty(result.Now);
        Assert.Single(result.Next);
        Assert.Empty(result.Previous);
    }

    [Fact]
    public async Task Handle_WhenFavoritePresentationsInPastOnly_ShouldReturnInPrevious()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var pastPresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            conferenceId: conferenceId,
            startDateTime: now.AddHours(-3),
            endDateTime: now.AddHours(-2),
            isFavorite: true);

        _repositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { pastPresentation }.AsReadOnly());

        var query = new GetConferenceScheduleNowQuery(profileId, conferenceId);
        var result = await _sut.Handle(query);

        Assert.Single(result.Previous);
        Assert.Empty(result.Now);
        Assert.Empty(result.Next);
    }

    [Fact]
    public async Task Handle_WhenCurrentAndPreviousAndNext_ShouldReturnAllThreeSlots()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var pastPresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            conferenceId: conferenceId,
            startDateTime: now.AddHours(-3),
            endDateTime: now.AddHours(-2),
            isFavorite: true);

        var currentPresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            conferenceId: conferenceId,
            startDateTime: now.AddMinutes(-30),
            endDateTime: now.AddMinutes(30),
            isFavorite: true);

        var nextPresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            conferenceId: conferenceId,
            startDateTime: now.AddHours(2),
            endDateTime: now.AddHours(3),
            isFavorite: true);

        _repositoryMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { pastPresentation, currentPresentation, nextPresentation }.AsReadOnly());

        var query = new GetConferenceScheduleNowQuery(profileId, conferenceId);
        var result = await _sut.Handle(query);

        Assert.Single(result.Previous);
        Assert.Single(result.Now);
        Assert.Single(result.Next);
    }
}
