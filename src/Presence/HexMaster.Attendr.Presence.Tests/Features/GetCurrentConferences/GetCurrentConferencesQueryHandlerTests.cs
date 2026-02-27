using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.GetCurrentConferences;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.GetCurrentConferences;

public sealed class GetCurrentConferencesQueryHandlerTests
{
    private readonly Mock<IConferencePresenceRepository> _repositoryMock;
    private readonly GetCurrentConferencesQueryHandler _sut;

    public GetCurrentConferencesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IConferencePresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new GetCurrentConferencesQueryHandler(
            _repositoryMock.Object,
            metrics,
            NullLogger<GetCurrentConferencesQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenConferenceIsCurrentAndAttending_ShouldReturnConference()
    {
        var profileId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var presence = ConferencePresenceFactory.Create(
            profileId: profileId,
            startDate: today.AddDays(-1),
            endDate: today.AddDays(1),
            isAttending: true,
            isFollowing: true);

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence> { presence }.AsReadOnly());

        var query = new GetCurrentConferencesQuery(profileId);
        var result = await _sut.Handle(query);

        Assert.Single(result);
        Assert.Equal(presence.ConferenceId, result[0].ConferenceId);
    }

    [Fact]
    public async Task Handle_WhenConferenceIsCurrentButNotAttending_ShouldReturnEmpty()
    {
        var profileId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var presence = ConferencePresenceFactory.Create(
            profileId: profileId,
            startDate: today.AddDays(-1),
            endDate: today.AddDays(1),
            isAttending: false,
            isFollowing: true);

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence> { presence }.AsReadOnly());

        var query = new GetCurrentConferencesQuery(profileId);
        var result = await _sut.Handle(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenConferenceIsInPast_ShouldReturnEmpty()
    {
        var profileId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var presence = ConferencePresenceFactory.Create(
            profileId: profileId,
            startDate: today.AddDays(-10),
            endDate: today.AddDays(-1),
            isAttending: true,
            isFollowing: true);

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence> { presence }.AsReadOnly());

        var query = new GetCurrentConferencesQuery(profileId);
        var result = await _sut.Handle(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenConferenceIsInFuture_ShouldReturnEmpty()
    {
        var profileId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var presence = ConferencePresenceFactory.Create(
            profileId: profileId,
            startDate: today.AddDays(5),
            endDate: today.AddDays(8),
            isAttending: true,
            isFollowing: true);

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence> { presence }.AsReadOnly());

        var query = new GetCurrentConferencesQuery(profileId);
        var result = await _sut.Handle(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenNoPresences_ShouldReturnEmpty()
    {
        var profileId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence>().AsReadOnly());

        var query = new GetCurrentConferencesQuery(profileId);
        var result = await _sut.Handle(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenMultipleMixed_ShouldReturnOnlyCurrentAndAttending()
    {
        var profileId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var current = ConferencePresenceFactory.Create(profileId: profileId, startDate: today.AddDays(-1), endDate: today.AddDays(1), isAttending: true);
        var past = ConferencePresenceFactory.Create(profileId: profileId, startDate: today.AddDays(-5), endDate: today.AddDays(-2), isAttending: true);
        var future = ConferencePresenceFactory.Create(profileId: profileId, startDate: today.AddDays(2), endDate: today.AddDays(5), isAttending: true);

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence> { current, past, future }.AsReadOnly());

        var query = new GetCurrentConferencesQuery(profileId);
        var result = await _sut.Handle(query);

        Assert.Single(result);
        Assert.Equal(current.ConferenceId, result[0].ConferenceId);
    }
}
