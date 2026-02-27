using Bogus;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Features.CreateTopic;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using HexMaster.Attendr.IntegrationEvents.Events.Topics;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.CreateTopic;

public class CreateTopicCommandHandlerTests
{
    private readonly Mock<ITopicsRepository> _mockTopicsRepository;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly Mock<ILogger<CreateTopicCommandHandler>> _mockLogger;
    private readonly CreateTopicCommandHandler _handler;
    private readonly Faker _faker;

    public CreateTopicCommandHandlerTests()
    {
        _mockTopicsRepository = new Mock<ITopicsRepository>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _mockLogger = new Mock<ILogger<CreateTopicCommandHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new CreateTopicCommandHandler(
            _mockTopicsRepository.Object,
            _mockEventPublisher.Object,
            metrics,
            _mockLogger.Object);

        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithIsManualFalse_ShouldCreateHiddenTopic()
    {
        // Arrange
        var command = new CreateTopicCommand("dotnet", ".NET", IsManual: false);
        var createdTopic = Topic.Create("dotnet", ".NET");

        _mockTopicsRepository.Setup(x => x.CreateTopicAsync(It.IsAny<Topic>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTopic);
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<TopicChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("dotnet", result.Key);
        Assert.Equal(".NET", result.Name);
        Assert.False(result.IsVisible);
        _mockTopicsRepository.Verify(x => x.CreateTopicAsync(
            It.Is<Topic>(t => !t.IsVisible), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithIsManualTrue_ShouldCreateVisibleTopic()
    {
        // Arrange
        var command = new CreateTopicCommand("azure", "Azure", IsManual: true);
        var createdTopic = Topic.CreateManually("azure", "Azure");

        _mockTopicsRepository.Setup(x => x.CreateTopicAsync(It.IsAny<Topic>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTopic);
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<TopicChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsVisible);
        _mockTopicsRepository.Verify(x => x.CreateTopicAsync(
            It.Is<Topic>(t => t.IsVisible), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishTopicChangedEvent()
    {
        // Arrange
        var command = new CreateTopicCommand("key", "Name");
        var createdTopic = Topic.Create("key", "Name");

        _mockTopicsRepository.Setup(x => x.CreateTopicAsync(It.IsAny<Topic>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTopic);
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<TopicChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockEventPublisher.Verify(x => x.PublishAsync(
            It.Is<TopicChangedEvent>(e => e.Key == "key" && e.Name == "Name"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagate()
    {
        var command = new CreateTopicCommand("key", "Name");
        _mockTopicsRepository.Setup(x => x.CreateTopicAsync(It.IsAny<Topic>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CreateTopicCommandHandler(
            null!,
            _mockEventPublisher.Object,
            TestMetricsFactory.CreateConferenceMetrics(),
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullEventPublisher_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CreateTopicCommandHandler(
            _mockTopicsRepository.Object,
            null!,
            TestMetricsFactory.CreateConferenceMetrics(),
            _mockLogger.Object));
    }
}
