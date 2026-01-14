using Bogus;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using HexMaster.Attendr.Conferences.UpdateConference;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.UpdateConference;

public class UpdateConferenceCommandHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly Mock<ILogger<UpdateConferenceCommandHandler>> _mockLogger;
    private readonly UpdateConferenceCommandHandler _handler;
    private readonly Faker _faker;

    public UpdateConferenceCommandHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _mockLogger = new Mock<ILogger<UpdateConferenceCommandHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new UpdateConferenceCommandHandler(
            _mockRepository.Object,
            _mockEventPublisher.Object,
            metrics,
            _mockLogger.Object);

        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateConference()
    {
        // Arrange
        var conference = ConferenceFactory.CreatePersistedConference();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(5);
        var command = new UpdateConferenceCommand(
            conference.Id,
            "Updated Title",
            "Updated City",
            "Updated Country",
            "https://updated.com/image.jpg",
            startDate,
            endDate,
            null,
            null);

        _mockRepository.Setup(x => x.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ConferenceUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(conference.Id, result.Id);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.City, result.City);
        Assert.Equal(command.Country, result.Country);

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<ConferenceUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConferenceNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var command = new UpdateConferenceCommand(
            conferenceId,
            "Title",
            "City",
            "Country",
            null,
            DateOnly.FromDateTime(_faker.Date.Future()),
            DateOnly.FromDateTime(_faker.Date.Future().AddDays(3)),
            null,
            null);

        _mockRepository.Setup(x => x.GetByIdAsync(conferenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conference?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSynchronizationSource_ShouldUpdateSyncSource()
    {
        // Arrange
        var conference = ConferenceFactory.CreatePersistedConference();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(5);
        var syncSourceDto = new SynchronizationSourceDto("Sessionize", "https://sessionize.com/api/v2/test");
        var command = new UpdateConferenceCommand(
            conference.Id,
            "Updated Title",
            "Updated City",
            "Updated Country",
            "https://updated.com/image.jpg",
            startDate,
            endDate,
            null,
            syncSourceDto);

        _mockRepository.Setup(x => x.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ConferenceUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(x => x.UpdateAsync(
            It.Is<Conference>(c => c.SynchronizationSource != null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullSynchronizationSource_ShouldRemoveSyncSource()
    {
        // Arrange
        var conference = ConferenceFactory.CreatePersistedConference();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(5);
        var command = new UpdateConferenceCommand(
            conference.Id,
            "Updated Title",
            "Updated City",
            "Updated Country",
            "https://updated.com/image.jpg",
            startDate,
            endDate,
            null,
            null);

        _mockRepository.Setup(x => x.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ConferenceUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishConferenceUpdatedEvent()
    {
        // Arrange
        var conference = ConferenceFactory.CreatePersistedConference();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(5);
        var command = new UpdateConferenceCommand(
            conference.Id,
            "Updated Title",
            "Updated City",
            "Updated Country",
            "https://updated.com/image.jpg",
            startDate,
            endDate,
            null,
            null);

        ConferenceUpdatedEvent? publishedEvent = null;
        _mockRepository.Setup(x => x.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ConferenceUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<ConferenceUpdatedEvent, CancellationToken>((evt, ct) => publishedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal(conference.Id, publishedEvent.ConferenceId);
        Assert.Equal(command.Title, publishedEvent.Title);
    }
}
