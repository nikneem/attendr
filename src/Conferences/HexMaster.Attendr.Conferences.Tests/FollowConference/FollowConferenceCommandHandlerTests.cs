using Bogus;
using HexMaster.Attendr.Conferences.Features.FollowConference;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.FollowConference;

public sealed class FollowConferenceCommandHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly Mock<ILogger<FollowConferenceCommandHandler>> _mockLogger;
    private readonly FollowConferenceCommandHandler _handler;
    private readonly Faker _faker;

    public FollowConferenceCommandHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _mockLogger = new Mock<ILogger<FollowConferenceCommandHandler>>();
        _handler = new FollowConferenceCommandHandler(
            _mockRepository.Object,
            _mockEventPublisher.Object,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPublishFollowEvent()
    {
        // Arrange
        var conference = ConferenceFactory.CreatePersistedConference();
        var profileId = Guid.NewGuid();
        var command = new FollowConferenceCommand(conference.Id, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ProfileFollowedConferenceEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(
            It.Is<ProfileFollowedConferenceEvent>(e =>
                e.ConferenceId == conference.Id &&
                e.ProfileId == profileId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConferenceNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var command = new FollowConferenceCommand(conferenceId, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(conferenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HexMaster.Attendr.Conferences.DomainModels.Conference?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockEventPublisher.Verify(x => x.PublishAsync(
            It.IsAny<ProfileFollowedConferenceEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var command = new FollowConferenceCommand(conferenceId, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(conferenceId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEventPublisherThrows_ShouldPropagateException()
    {
        // Arrange
        var conference = ConferenceFactory.CreatePersistedConference();
        var profileId = Guid.NewGuid();
        var command = new FollowConferenceCommand(conference.Id, profileId);

        _mockRepository.Setup(x => x.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ProfileFollowedConferenceEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Event bus error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
