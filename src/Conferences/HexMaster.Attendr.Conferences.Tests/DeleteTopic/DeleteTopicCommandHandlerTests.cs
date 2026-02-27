using HexMaster.Attendr.Conferences.Features.DeleteTopic;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.DeleteTopic;

public class DeleteTopicCommandHandlerTests
{
    private readonly Mock<ITopicsRepository> _mockTopicsRepository;
    private readonly Mock<ILogger<DeleteTopicCommandHandler>> _mockLogger;
    private readonly DeleteTopicCommandHandler _handler;

    public DeleteTopicCommandHandlerTests()
    {
        _mockTopicsRepository = new Mock<ITopicsRepository>();
        _mockLogger = new Mock<ILogger<DeleteTopicCommandHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new DeleteTopicCommandHandler(
            _mockTopicsRepository.Object,
            metrics,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTopicExists_ShouldCascadeDeleteAndReturnTrue()
    {
        var id = Guid.NewGuid();
        _mockTopicsRepository.Setup(x => x.DeleteTopicPresentationReferencesAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockTopicsRepository.Setup(x => x.DeleteTopicAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteTopicCommand(id), CancellationToken.None);

        Assert.True(result);
        _mockTopicsRepository.Verify(x => x.DeleteTopicPresentationReferencesAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mockTopicsRepository.Verify(x => x.DeleteTopicAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTopicNotFound_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        _mockTopicsRepository.Setup(x => x.DeleteTopicPresentationReferencesAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockTopicsRepository.Setup(x => x.DeleteTopicAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(new DeleteTopicCommand(id), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_ShouldAlwaysDeletePresentationReferencesBeforeTopic()
    {
        var id = Guid.NewGuid();
        var callOrder = new List<string>();

        _mockTopicsRepository.Setup(x => x.DeleteTopicPresentationReferencesAsync(id, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("references"))
            .Returns(Task.CompletedTask);
        _mockTopicsRepository.Setup(x => x.DeleteTopicAsync(id, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("topic"))
            .ReturnsAsync(true);

        await _handler.Handle(new DeleteTopicCommand(id), CancellationToken.None);

        Assert.Equal(new[] { "references", "topic" }, callOrder);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagate()
    {
        var id = Guid.NewGuid();
        _mockTopicsRepository.Setup(x => x.DeleteTopicPresentationReferencesAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new DeleteTopicCommand(id), CancellationToken.None));
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DeleteTopicCommandHandler(
            null!,
            TestMetricsFactory.CreateConferenceMetrics(),
            _mockLogger.Object));
    }
}
