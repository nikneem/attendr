using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.RatePresentation;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.RatePresentation;

public sealed class RatePresentationCommandHandlerTests
{
    private readonly Mock<IPresentationPresenceRepository> _repositoryMock;
    private readonly Mock<IIntegrationEventPublisher> _publisherMock;
    private readonly RatePresentationCommandHandler _sut;

    public RatePresentationCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPresentationPresenceRepository>();
        _publisherMock = new Mock<IIntegrationEventPublisher>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new RatePresentationCommandHandler(
            _repositoryMock.Object,
            metrics,
            _publisherMock.Object,
            NullLogger<RatePresentationCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenPresentationFound_ShouldRateAndUpdate()
    {
        var presentation = PresentationPresenceFactory.Create();
        var command = new RatePresentationCommand(
            presentation.ProfileId,
            presentation.ConferenceId,
            presentation.PresentationId,
            new RatePresentationDto((byte)4, false));

        _repositoryMock
            .Setup(r => r.GetByIdAsync(presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId, default))
            .ReturnsAsync(presentation);

        await _sut.Handle(command);

        Assert.True(presentation.IsRated);
        Assert.Equal((byte)4, presentation.Rating);
        _repositoryMock.Verify(r =>
            r.UpdateAsync(presentation.ProfileId, presentation.ConferenceId, presentation, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPresentationNotFound_ShouldThrowInvalidOperationException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((PresentationPresence?)null);

        var command = new RatePresentationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new RatePresentationDto((byte)3, false));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenRatingDtoIsNull_ShouldThrowArgumentNullException()
    {
        var command = new RatePresentationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null!);

        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenIsFavoriteAndHasTopics_ShouldPublishTopicInterestEvent()
    {
        var topic = new PresentationTopic("dotnet", ".NET");
        var presentation = PresentationPresenceFactory.Create(topics: new[] { topic });
        var command = new RatePresentationCommand(
            presentation.ProfileId,
            presentation.ConferenceId,
            presentation.PresentationId,
            new RatePresentationDto((byte)5, true));

        _repositoryMock
            .Setup(r => r.GetByIdAsync(presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId, default))
            .ReturnsAsync(presentation);

        await _sut.Handle(command);

        _publisherMock.Verify(p =>
            p.PublishAsync(It.IsAny<HexMaster.Attendr.IntegrationEvents.Events.Profiles.ProfileTopicInterestEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFavorite_ShouldNotPublishTopicInterestEvent()
    {
        var topic = new PresentationTopic("dotnet", ".NET");
        var presentation = PresentationPresenceFactory.Create(topics: new[] { topic });
        var command = new RatePresentationCommand(
            presentation.ProfileId,
            presentation.ConferenceId,
            presentation.PresentationId,
            new RatePresentationDto((byte)3, false));

        _repositoryMock
            .Setup(r => r.GetByIdAsync(presentation.ProfileId, presentation.ConferenceId, presentation.PresentationId, default))
            .ReturnsAsync(presentation);

        await _sut.Handle(command);

        _publisherMock.Verify(p =>
            p.PublishAsync(It.IsAny<HexMaster.Attendr.IntegrationEvents.Events.Profiles.ProfileTopicInterestEvent>(), default),
            Times.Never);
    }
}
