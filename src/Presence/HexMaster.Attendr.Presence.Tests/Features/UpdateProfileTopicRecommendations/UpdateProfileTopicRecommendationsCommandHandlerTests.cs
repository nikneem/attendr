using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.UpdateProfileTopicRecommendations;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.UpdateProfileTopicRecommendations;

public sealed class UpdateProfileTopicRecommendationsCommandHandlerTests
{
    private readonly Mock<IPresentationPresenceRepository> _repositoryMock;
    private readonly UpdateProfileTopicRecommendationsCommandHandler _sut;

    public UpdateProfileTopicRecommendationsCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new UpdateProfileTopicRecommendationsCommandHandler(
            _repositoryMock.Object,
            metrics,
            NullLogger<UpdateProfileTopicRecommendationsCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Handle(null!));
    }

    [Fact]
    public async Task Handle_WhenNoTopicsWithHighWeight_ShouldReturnZero()
    {
        var profileId = Guid.NewGuid();
        var command = new UpdateProfileTopicRecommendationsCommand(
            profileId,
            new List<ProfileTopicWeight>
            {
                new("dotnet", 50),
                new("csharp", 30)
            });

        var result = await _sut.Handle(command);

        Assert.Equal(0, result);
        _repositoryMock.Verify(r => r.GetByProfileAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WithHighWeightTopics_ShouldSetRecommendedForMatchingPresentations()
    {
        var profileId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(1);

        var matchingPresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            topics: new[] { new PresentationTopic("dotnet", ".NET") },
            startDateTime: start, endDateTime: end,
            isRecommended: false);
        var nonMatchingPresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            topics: new[] { new PresentationTopic("java", "Java") },
            startDateTime: start.AddHours(2), endDateTime: end.AddHours(2),
            isRecommended: false);

        _repositoryMock
            .Setup(r => r.GetByProfileAsync(profileId, default))
            .ReturnsAsync(new List<PresentationPresence> { matchingPresentation, nonMatchingPresentation }.AsReadOnly());

        var command = new UpdateProfileTopicRecommendationsCommand(
            profileId,
            new List<ProfileTopicWeight> { new("dotnet", 80) });

        var result = await _sut.Handle(command);

        Assert.Equal(1, result);
        Assert.True(matchingPresentation.IsRecommended);
        Assert.False(nonMatchingPresentation.IsRecommended);
        _repositoryMock.Verify(r =>
            r.UpdateAsync(profileId, It.IsAny<Guid>(), matchingPresentation, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPresentationWasRecommendedButTopicNoLongerMatches_ShouldUnsetRecommended()
    {
        var profileId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(1);

        var presentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            topics: new[] { new PresentationTopic("java", "Java") },
            startDateTime: start, endDateTime: end,
            isRecommended: true);  // was recommended but topic no longer matches

        _repositoryMock
            .Setup(r => r.GetByProfileAsync(profileId, default))
            .ReturnsAsync(new List<PresentationPresence> { presentation }.AsReadOnly());

        var command = new UpdateProfileTopicRecommendationsCommand(
            profileId,
            new List<ProfileTopicWeight> { new("dotnet", 80) }); // dotnet, not java

        var result = await _sut.Handle(command);

        Assert.Equal(1, result);
        Assert.False(presentation.IsRecommended);
    }

    [Fact]
    public async Task Handle_WhenPresentationIsInThePast_ShouldSkipIt()
    {
        var profileId = Guid.NewGuid();
        var pastPresentation = PresentationPresenceFactory.Create(
            profileId: profileId,
            topics: new[] { new PresentationTopic("dotnet", ".NET") },
            startDateTime: DateTimeOffset.UtcNow.AddHours(-2),
            endDateTime: DateTimeOffset.UtcNow.AddHours(-1),
            isRecommended: false);

        _repositoryMock
            .Setup(r => r.GetByProfileAsync(profileId, default))
            .ReturnsAsync(new List<PresentationPresence> { pastPresentation }.AsReadOnly());

        var command = new UpdateProfileTopicRecommendationsCommand(
            profileId,
            new List<ProfileTopicWeight> { new("dotnet", 90) });

        var result = await _sut.Handle(command);

        Assert.Equal(0, result);
        Assert.False(pastPresentation.IsRecommended);
    }
}
