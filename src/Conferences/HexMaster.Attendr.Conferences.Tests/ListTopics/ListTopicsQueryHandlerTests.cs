using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Features.ListTopics;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.ListTopics;

public class ListTopicsQueryHandlerTests
{
    private readonly Mock<ITopicsRepository> _mockTopicsRepository;
    private readonly Mock<ILogger<ListTopicsQueryHandler>> _mockLogger;
    private readonly ListTopicsQueryHandler _handler;

    public ListTopicsQueryHandlerTests()
    {
        _mockTopicsRepository = new Mock<ITopicsRepository>();
        _mockLogger = new Mock<ILogger<ListTopicsQueryHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new ListTopicsQueryHandler(
            _mockTopicsRepository.Object,
            metrics,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithTopics_ShouldReturnAllSortedByKey()
    {
        var topics = new List<Topic>
        {
            Topic.FromPersisted(Guid.NewGuid(), "csharp", "C#", isVisible: true, DateTimeOffset.UtcNow),
            Topic.FromPersisted(Guid.NewGuid(), "azure", "Azure", isVisible: true, DateTimeOffset.UtcNow),
            Topic.FromPersisted(Guid.NewGuid(), "dotnet", ".NET", isVisible: true, DateTimeOffset.UtcNow),
        };

        _mockTopicsRepository.Setup(x => x.ListTopicsAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topics);

        var result = await _handler.Handle(new ListTopicsQuery(OnlyVisible: true), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Topics.Count);
        // Verify sorted by key
        Assert.Equal("azure", result.Topics[0].Key);
        Assert.Equal("csharp", result.Topics[1].Key);
        Assert.Equal("dotnet", result.Topics[2].Key);
    }

    [Fact]
    public async Task Handle_WithOnlyVisibleFalse_ShouldPassFlagToRepository()
    {
        _mockTopicsRepository.Setup(x => x.ListTopicsAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topic>());

        await _handler.Handle(new ListTopicsQuery(OnlyVisible: false), CancellationToken.None);

        _mockTopicsRepository.Verify(x => x.ListTopicsAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoTopics_ShouldReturnEmptyResult()
    {
        _mockTopicsRepository.Setup(x => x.ListTopicsAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topic>());

        var result = await _handler.Handle(new ListTopicsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Topics);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagate()
    {
        _mockTopicsRepository.Setup(x => x.ListTopicsAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new ListTopicsQuery(), CancellationToken.None));
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ListTopicsQueryHandler(
            null!,
            TestMetricsFactory.CreateConferenceMetrics(),
            _mockLogger.Object));
    }
}
