using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Features.GetMetrics;
using HexMaster.Attendr.Core.Cache;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.GetMetrics;

public class GetMetricsQueryHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<IAttendrCacheClient> _mockCacheClient;
    private readonly GetMetricsQueryHandler _handler;

    public GetMetricsQueryHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockCacheClient = new Mock<IAttendrCacheClient>();

        _handler = new GetMetricsQueryHandler(_mockRepository.Object, _mockCacheClient.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheReturnsMetrics_ShouldReturnMetricsDto()
    {
        // Arrange
        var expectedDto = new ConferenceMetricsDto(10, 50, 200, 40, 5, 15);

        _mockCacheClient
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<ConferenceMetricsDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(new GetMetricsQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto, result);
    }

    [Fact]
    public async Task Handle_ShouldUseConferencesMetricsCacheKey()
    {
        // Arrange
        var expectedDto = new ConferenceMetricsDto(0, 0, 0, 0, 0, 0);

        string? capturedKey = null;
        _mockCacheClient
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<ConferenceMetricsDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, Task<ConferenceMetricsDto?>>, TimeSpan?, CancellationToken>(
                (key, _, _, _) => capturedKey = key)
            .ReturnsAsync(expectedDto);

        // Act
        await _handler.Handle(new GetMetricsQuery(), CancellationToken.None);

        // Assert
        Assert.Equal("conferences:metrics", capturedKey);
    }

    [Fact]
    public async Task Handle_WhenCacheMisses_ShouldCallRepository()
    {
        // Arrange
        var metricsDto = new ConferenceMetricsDto(5, 20, 100, 30, 3, 8);

        _mockRepository.Setup(x => x.GetMetricsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metricsDto);

        _mockCacheClient
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<ConferenceMetricsDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<CancellationToken, Task<ConferenceMetricsDto?>>, TimeSpan?, CancellationToken>(
                async (_, factory, _, ct) => await factory(ct));

        // Act
        var result = await _handler.Handle(new GetMetricsQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(x => x.GetMetricsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseTtlOfOneHour()
    {
        // Arrange
        var expectedDto = new ConferenceMetricsDto(0, 0, 0, 0, 0, 0);

        TimeSpan? capturedTtl = null;
        _mockCacheClient
            .Setup(x => x.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<ConferenceMetricsDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<CancellationToken, Task<ConferenceMetricsDto?>>, TimeSpan?, CancellationToken>(
                (_, _, ttl, _) => capturedTtl = ttl)
            .ReturnsAsync(expectedDto);

        // Act
        await _handler.Handle(new GetMetricsQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(TimeSpan.FromHours(1), capturedTtl);
    }
}
