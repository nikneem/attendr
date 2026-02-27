using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.RatePresentation;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.RatePresentation;

public sealed class GetRandomPresentationToRateQueryHandlerTests
{
    private readonly Mock<IPresentationPresenceRepository> _repositoryMock;
    private readonly GetRandomPresentationToRateQueryHandler _sut;

    public GetRandomPresentationToRateQueryHandlerTests()
    {
        _repositoryMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new GetRandomPresentationToRateQueryHandler(
            _repositoryMock.Object,
            metrics,
            NullLogger<GetRandomPresentationToRateQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenNoUnratedPresentations_ShouldReturnNull()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetUnratedByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence>().AsReadOnly());

        var query = new GetRandomPresentationToRateQuery(profileId, conferenceId, 0);
        var result = await _sut.Handle(query);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenIndexOutOfRange_ShouldReturnNull()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        var presentations = PresentationPresenceFactory.CreateList(2, profileId: profileId, conferenceId: conferenceId);

        _repositoryMock
            .Setup(r => r.GetUnratedByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(presentations.AsReadOnly());

        var query = new GetRandomPresentationToRateQuery(profileId, conferenceId, 5);
        var result = await _sut.Handle(query);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenIndexIsValid_ShouldReturnPresentation()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();

        var presentations = PresentationPresenceFactory.CreateList(3, profileId: profileId, conferenceId: conferenceId);

        _repositoryMock
            .Setup(r => r.GetUnratedByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(presentations.AsReadOnly());

        var query = new GetRandomPresentationToRateQuery(profileId, conferenceId, 0);
        var result = await _sut.Handle(query);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Handle_WhenPresentationHasSpeakers_ShouldMapSpeakers()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var speaker = new PresentationSpeaker(Guid.NewGuid(), "Test Speaker", null);

        var presentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            conferenceId: conferenceId,
            speakers: new[] { speaker });

        _repositoryMock
            .Setup(r => r.GetUnratedByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { presentation }.AsReadOnly());

        var query = new GetRandomPresentationToRateQuery(profileId, conferenceId, 0);
        var result = await _sut.Handle(query);

        Assert.NotNull(result);
        Assert.Single(result.Speakers);
        Assert.Equal("Test Speaker", result.Speakers.First().Name);
    }

    [Fact]
    public async Task Handle_WhenPresentationHasTopics_ShouldMapTopics()
    {
        var profileId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var topic = new PresentationTopic("dotnet", ".NET");

        var presentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            conferenceId: conferenceId,
            topics: new[] { topic });

        _repositoryMock
            .Setup(r => r.GetUnratedByProfileAndConferenceAsync(profileId, conferenceId, default))
            .ReturnsAsync(new List<PresentationPresence> { presentation }.AsReadOnly());

        var query = new GetRandomPresentationToRateQuery(profileId, conferenceId, 0);
        var result = await _sut.Handle(query);

        Assert.NotNull(result);
        Assert.Single(result.Topics);
        Assert.Equal("dotnet", result.Topics.First().Key);
    }
}
