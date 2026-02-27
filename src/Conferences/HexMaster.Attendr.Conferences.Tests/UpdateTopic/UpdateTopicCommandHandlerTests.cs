using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Features.UpdateTopic;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Events.Topics;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.UpdateTopic;

public class UpdateTopicCommandHandlerTests
{
    private readonly Mock<ITopicsRepository> _mockTopicsRepository;
    private readonly Mock<IConferenceRepository> _mockConferenceRepository;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly Mock<ILogger<UpdateTopicCommandHandler>> _mockLogger;
    private readonly UpdateTopicCommandHandler _handler;

    public UpdateTopicCommandHandlerTests()
    {
        _mockTopicsRepository = new Mock<ITopicsRepository>();
        _mockConferenceRepository = new Mock<IConferenceRepository>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _mockLogger = new Mock<ILogger<UpdateTopicCommandHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new UpdateTopicCommandHandler(
            _mockTopicsRepository.Object,
            _mockConferenceRepository.Object,
            _mockEventPublisher.Object,
            metrics,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateTopicAndPublishEvent()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var topic = Topic.FromPersisted(topicId, "oldkey", "Old Name", isVisible: false, DateTimeOffset.UtcNow);
        var command = new UpdateTopicCommand(topicId, "newkey", "New Name", IsVisible: false);

        SetupRepository(topicId, topic);
        SetupNoAffectedPresentations(topicId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newkey", result.Key);
        Assert.Equal("New Name", result.Name);
        _mockTopicsRepository.Verify(x => x.UpdateTopicAsync(topic, It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<TopicChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTopicNotFound_ShouldThrowKeyNotFoundException()
    {
        var topicId = Guid.NewGuid();
        _mockTopicsRepository.Setup(x => x.GetTopicByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Topic?)null);

        var command = new UpdateTopicCommand(topicId, "key", "Name", IsVisible: false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _mockTopicsRepository.Verify(x => x.UpdateTopicAsync(It.IsAny<Topic>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<TopicChangedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMakingTopicVisible_ShouldCallMakeVisible()
    {
        var topicId = Guid.NewGuid();
        var topic = Topic.FromPersisted(topicId, "key", "Name", isVisible: false, DateTimeOffset.UtcNow);
        var command = new UpdateTopicCommand(topicId, "key", "Name", IsVisible: true);

        SetupRepository(topicId, topic);
        SetupNoAffectedPresentations(topicId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsVisible);
    }

    [Fact]
    public async Task Handle_WhenHidingTopic_ShouldCallHide()
    {
        var topicId = Guid.NewGuid();
        var topic = Topic.FromPersisted(topicId, "key", "Name", isVisible: true, DateTimeOffset.UtcNow);
        var command = new UpdateTopicCommand(topicId, "key", "Name", IsVisible: false);

        SetupRepository(topicId, topic);
        SetupNoAffectedPresentations(topicId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsVisible);
    }

    [Fact]
    public async Task Handle_WhenVisibilityAlreadyMatches_ShouldNotChangeTopic()
    {
        var topicId = Guid.NewGuid();
        var topic = Topic.FromPersisted(topicId, "key", "Name", isVisible: true, DateTimeOffset.UtcNow);
        var command = new UpdateTopicCommand(topicId, "key", "Name", IsVisible: true);

        SetupRepository(topicId, topic);
        SetupNoAffectedPresentations(topicId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsVisible);
    }

    [Fact]
    public async Task Handle_WithAffectedPresentations_ShouldPublishPresentationUpdatedEvent()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();

        var topic = Topic.FromPersisted(topicId, "key", "Name", isVisible: true, DateTimeOffset.UtcNow);
        var command = new UpdateTopicCommand(topicId, "key", "Name", IsVisible: true);

        SetupRepository(topicId, topic);

        _mockTopicsRepository.Setup(x => x.GetFuturePresentationsByTopicIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Guid, Guid)> { (conferenceId, presentationId) });

        var room = ConferenceFactory.CreateRoom("Hall A", 100);
        var speaker = ConferenceFactory.CreateSpeaker("Alice", null);
        var presentation = Presentation.FromPersisted(
            presentationId, "Talk Title", "Abstract",
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            room, new[] { speaker });

        _mockConferenceRepository.Setup(x => x.GetPresentationByIdAsync(conferenceId, presentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(presentation);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<TopicChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<PresentationUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockEventPublisher.Verify(x => x.PublishAsync(
            It.Is<PresentationUpdatedEvent>(e => e.PresentationId == presentationId && !e.IsScheduleChanged),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAffectedPresentationNotFound_ShouldSkipAndContinue()
    {
        var topicId = Guid.NewGuid();
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();

        var topic = Topic.FromPersisted(topicId, "key", "Name", isVisible: true, DateTimeOffset.UtcNow);
        var command = new UpdateTopicCommand(topicId, "key", "Name", IsVisible: true);

        SetupRepository(topicId, topic);

        _mockTopicsRepository.Setup(x => x.GetFuturePresentationsByTopicIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Guid, Guid)> { (conferenceId, presentationId) });

        _mockConferenceRepository.Setup(x => x.GetPresentationByIdAsync(conferenceId, presentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Presentation?)null);

        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<TopicChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act – should not throw
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        // No PresentationUpdatedEvent because presentation was not found
        _mockEventPublisher.Verify(x => x.PublishAsync(
            It.IsAny<PresentationUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagate()
    {
        var topicId = Guid.NewGuid();
        _mockTopicsRepository.Setup(x => x.GetTopicByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new UpdateTopicCommand(topicId, "key", "Name", IsVisible: false), CancellationToken.None));
    }

    [Fact]
    public void Constructor_WithNullTopicsRepository_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UpdateTopicCommandHandler(
            null!,
            _mockConferenceRepository.Object,
            _mockEventPublisher.Object,
            TestMetricsFactory.CreateConferenceMetrics(),
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullConferenceRepository_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UpdateTopicCommandHandler(
            _mockTopicsRepository.Object,
            null!,
            _mockEventPublisher.Object,
            TestMetricsFactory.CreateConferenceMetrics(),
            _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullEventPublisher_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UpdateTopicCommandHandler(
            _mockTopicsRepository.Object,
            _mockConferenceRepository.Object,
            null!,
            TestMetricsFactory.CreateConferenceMetrics(),
            _mockLogger.Object));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void SetupRepository(Guid topicId, Topic topic)
    {
        _mockTopicsRepository.Setup(x => x.GetTopicByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _mockTopicsRepository.Setup(x => x.UpdateTopicAsync(topic, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockEventPublisher.Setup(x => x.PublishAsync(It.IsAny<TopicChangedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupNoAffectedPresentations(Guid topicId)
    {
        _mockTopicsRepository.Setup(x => x.GetFuturePresentationsByTopicIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(Guid, Guid)>());
    }
}
