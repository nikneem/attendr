using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Features.GetTopic;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.GetTopic;

public class GetTopicQueryHandlerTests
{
    private readonly Mock<ITopicsRepository> _mockTopicsRepository;
    private readonly Mock<ILogger<GetTopicQueryHandler>> _mockLogger;
    private readonly GetTopicQueryHandler _handler;

    public GetTopicQueryHandlerTests()
    {
        _mockTopicsRepository = new Mock<ITopicsRepository>();
        _mockLogger = new Mock<ILogger<GetTopicQueryHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new GetTopicQueryHandler(
            _mockTopicsRepository.Object,
            metrics,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenTopicExists_ShouldReturnTopicDto()
    {
        var id = Guid.NewGuid();
        var topic = Topic.FromPersisted(id, "dotnet", ".NET", isVisible: true, DateTimeOffset.UtcNow);

        _mockTopicsRepository.Setup(x => x.GetTopicByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);

        var result = await _handler.Handle(new GetTopicQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("dotnet", result.Key);
        Assert.Equal(".NET", result.Name);
        Assert.True(result.IsVisible);
    }

    [Fact]
    public async Task Handle_WhenTopicNotFound_ShouldReturnNull()
    {
        var id = Guid.NewGuid();
        _mockTopicsRepository.Setup(x => x.GetTopicByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Topic?)null);

        var result = await _handler.Handle(new GetTopicQuery(id), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagate()
    {
        var id = Guid.NewGuid();
        _mockTopicsRepository.Setup(x => x.GetTopicByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new GetTopicQuery(id), CancellationToken.None));
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GetTopicQueryHandler(
            null!,
            TestMetricsFactory.CreateConferenceMetrics(),
            _mockLogger.Object));
    }
}
