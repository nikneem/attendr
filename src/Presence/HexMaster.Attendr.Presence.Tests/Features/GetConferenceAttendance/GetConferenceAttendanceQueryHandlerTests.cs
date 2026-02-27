using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.GetConferenceAttendance;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.GetConferenceAttendance;

public sealed class GetConferenceAttendanceQueryHandlerTests
{
    private readonly Mock<IConferencePresenceRepository> _conferenceMock;
    private readonly Mock<IPresentationPresenceRepository> _presentationMock;
    private readonly GetConferenceAttendanceQueryHandler _sut;

    public GetConferenceAttendanceQueryHandlerTests()
    {
        _conferenceMock = new Mock<IConferencePresenceRepository>();
        _presentationMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new GetConferenceAttendanceQueryHandler(
            _conferenceMock.Object,
            _presentationMock.Object,
            metrics,
            NullLogger<GetConferenceAttendanceQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenNotFollowingConference_ShouldReturnIsFollowingFalse()
    {
        var query = new GetConferenceAttendanceQuery(Guid.NewGuid(), Guid.NewGuid());

        _conferenceMock
            .Setup(r => r.GetAsync(query.ConferenceId, query.ProfileId, default))
            .ReturnsAsync((ConferencePresence?)null);

        var result = await _sut.Handle(query);

        Assert.NotNull(result);
        Assert.False(result.IsFollowing);
        Assert.False(result.IsAttending);
        Assert.Empty(result.FavoritePresentationIds);
        Assert.Empty(result.RecommendedPresentationIds);
    }

    [Fact]
    public async Task Handle_WhenFollowingWithFavorites_ShouldReturnFavoritePresentationIds()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var query = new GetConferenceAttendanceQuery(profileId, conferenceId);

        var conferencePresence = ConferencePresenceFactory.Create(profileId: profileId, conferenceId: conferenceId);
        var favPres = PresentationPresenceFactory.Create(profileId: profileId, conferenceId: conferenceId, isFavorite: true);
        var nonFavPres = PresentationPresenceFactory.Create(profileId: profileId, conferenceId: conferenceId, isFavorite: false);

        _conferenceMock
            .Setup(r => r.GetAsync(conferenceId, profileId, default))
            .ReturnsAsync(conferencePresence);
        _presentationMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { favPres, nonFavPres }.AsReadOnly());

        var result = await _sut.Handle(query);

        Assert.True(result.IsFollowing);
        Assert.Single(result.FavoritePresentationIds);
        Assert.Contains(favPres.PresentationId, result.FavoritePresentationIds);
    }

    [Fact]
    public async Task Handle_WhenFollowingWithRecommended_ShouldReturnRecommendedIds()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var query = new GetConferenceAttendanceQuery(profileId, conferenceId);

        var conferencePresence = ConferencePresenceFactory.Create(profileId: profileId, conferenceId: conferenceId);
        var recPres = PresentationPresenceFactory.Create(profileId: profileId, conferenceId: conferenceId, isRecommended: true);

        _conferenceMock
            .Setup(r => r.GetAsync(conferenceId, profileId, default))
            .ReturnsAsync(conferencePresence);
        _presentationMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { recPres }.AsReadOnly());

        var result = await _sut.Handle(query);

        Assert.Single(result.RecommendedPresentationIds);
        Assert.Contains(recPres.PresentationId, result.RecommendedPresentationIds);
    }

    [Fact]
    public async Task Handle_WhenFollowingWithNoPresentations_ShouldReturnEmptyCollections()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var query = new GetConferenceAttendanceQuery(profileId, conferenceId);

        var conferencePresence = ConferencePresenceFactory.Create(profileId: profileId, conferenceId: conferenceId);

        _conferenceMock
            .Setup(r => r.GetAsync(conferenceId, profileId, default))
            .ReturnsAsync(conferencePresence);
        _presentationMock
            .Setup(r => r.GetByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence>().AsReadOnly());

        var result = await _sut.Handle(query);

        Assert.True(result.IsFollowing);
        Assert.Empty(result.FavoritePresentationIds);
        Assert.Empty(result.RecommendedPresentationIds);
    }
}
