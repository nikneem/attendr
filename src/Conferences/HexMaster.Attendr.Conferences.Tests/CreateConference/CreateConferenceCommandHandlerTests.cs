using Bogus;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Features.CreateConference;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.CreateConference;

public class CreateConferenceCommandHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly Mock<ILogger<CreateConferenceCommandHandler>> _mockLogger;
    private readonly CreateConferenceCommandHandler _handler;
    private readonly Faker _faker;

    public CreateConferenceCommandHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _mockLogger = new Mock<ILogger<CreateConferenceCommandHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new CreateConferenceCommandHandler(
            _mockRepository.Object,
            _mockEventPublisher.Object,
            metrics,
            _mockLogger.Object);

        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateConference()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);
        var command = new CreateConferenceCommand(
            _faker.Company.CompanyName() + " Conference",
            _faker.Address.City(),
            _faker.Address.Country(),
            _faker.Internet.Url(),
            startDate,
            endDate,
            null);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ConferenceCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.City, result.City);
        Assert.Equal(command.Country, result.Country);
        Assert.Equal(command.StartDate, result.StartDate);
        Assert.Equal(command.EndDate, result.EndDate);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<ConferenceCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSynchronizationSource_ShouldCreateConferenceWithSyncSource()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);
        var syncSourceDto = new SynchronizationSourceDto("Sessionize", "https://sessionize.com/api/v2/test");
        var command = new CreateConferenceCommand(
            _faker.Company.CompanyName() + " Conference",
            _faker.Address.City(),
            _faker.Address.Country(),
            _faker.Internet.Url(),
            startDate,
            endDate,
            syncSourceDto);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ConferenceCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(x => x.AddAsync(
            It.Is<Conference>(c => c.SynchronizationSource != null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidSyncSourceType_ShouldThrowArgumentException()
    {
        // Arrange
        var syncSourceDto = new SynchronizationSourceDto("InvalidType", "https://test.com");
        var command = new CreateConferenceCommand(
            _faker.Company.CompanyName() + " Conference",
            _faker.Address.City(),
            _faker.Address.Country(),
            _faker.Internet.Url(),
            DateOnly.FromDateTime(_faker.Date.Future()),
            DateOnly.FromDateTime(_faker.Date.Future().AddDays(3)),
            syncSourceDto);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);
        var command = new CreateConferenceCommand(
            _faker.Company.CompanyName() + " Conference",
            _faker.Address.City(),
            _faker.Address.Country(),
            _faker.Internet.Url(),
            startDate,
            endDate,
            null);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldPublishConferenceCreatedEvent()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);
        var command = new CreateConferenceCommand(
            _faker.Company.CompanyName() + " Conference",
            _faker.Address.City(),
            _faker.Address.Country(),
            _faker.Internet.Url(),
            startDate,
            endDate,
            null);

        ConferenceCreatedEvent? publishedEvent = null;
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ConferenceCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<ConferenceCreatedEvent, CancellationToken>((evt, ct) => publishedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal(result.Id, publishedEvent.ConferenceId);
        Assert.Equal(command.Title, publishedEvent.Title);
        Assert.Equal(command.City, publishedEvent.City);
        Assert.Equal(command.Country, publishedEvent.Country);
        Assert.Equal(command.StartDate, publishedEvent.StartDate);
        Assert.Equal(command.EndDate, publishedEvent.EndDate);
    }

    [Fact]
    public async Task Handle_WithCreatedByProfileId_ShouldStoreOwnerOnConference()
    {
        // Arrange
        var ownerProfileId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);
        var command = new CreateConferenceCommand(
            _faker.Company.CompanyName() + " Conference",
            _faker.Address.City(),
            _faker.Address.Country(),
            null,
            startDate,
            endDate,
            null,
            ownerProfileId);

        Conference? capturedConference = null;
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .Callback<Conference, CancellationToken>((conf, ct) => capturedConference = conf)
            .Returns(Task.CompletedTask);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<ConferenceCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedConference);
        Assert.Equal(ownerProfileId, capturedConference!.CreatedByProfileId);
    }
}
