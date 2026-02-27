using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.GetMyConferences;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.GetMyConferences;

public sealed class GetMyConferencesQueryHandlerTests
{
    private readonly Mock<IConferencePresenceRepository> _repositoryMock;
    private readonly GetMyConferencesQueryHandler _sut;

    public GetMyConferencesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IConferencePresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new GetMyConferencesQueryHandler(
            _repositoryMock.Object,
            metrics,
            NullLogger<GetMyConferencesQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithFutureConferences_ShouldReturnThem()
    {
        var profileId = Guid.NewGuid();
        var query = new GetMyConferencesQuery(profileId);
        var futureConferences = ConferencePresenceFactory.CreateList(3, profileId: profileId, futureOnly: true);

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(futureConferences.AsReadOnly());

        var result = await _sut.Handle(query);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Handle_WithPastConferences_ShouldFilterThemOut()
    {
        var profileId = Guid.NewGuid();
        var query = new GetMyConferencesQuery(profileId);
        var pastConference = ConferencePresenceFactory.Create(
            profileId: profileId,
            startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            endDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)));

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence> { pastConference }.AsReadOnly());

        var result = await _sut.Handle(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithNoConferences_ShouldReturnEmptyList()
    {
        var profileId = Guid.NewGuid();
        var query = new GetMyConferencesQuery(profileId);

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence>().AsReadOnly());

        var result = await _sut.Handle(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithMixedConferences_ShouldReturnOnlyFutureOrderedByStartDate()
    {
        var profileId = Guid.NewGuid();
        var query = new GetMyConferencesQuery(profileId);

        var pastConference = ConferencePresenceFactory.Create(
            profileId: profileId,
            startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            endDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)));
        var futureConf1 = ConferencePresenceFactory.Create(
            profileId: profileId,
            startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            endDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(11)));
        var futureConf2 = ConferencePresenceFactory.Create(
            profileId: profileId,
            startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            endDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)));

        _repositoryMock
            .Setup(r => r.GetByProfileIdAsync(profileId, default))
            .ReturnsAsync(new List<ConferencePresence> { pastConference, futureConf1, futureConf2 }.AsReadOnly());

        var result = await _sut.Handle(query);

        Assert.Equal(2, result.Count);
        Assert.Equal(futureConf2.ConferenceId, result[0].ConferenceId);
        Assert.Equal(futureConf1.ConferenceId, result[1].ConferenceId);
    }
}
