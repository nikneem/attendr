using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Services;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.Services;

public class PresentationTopicsAnalysisServiceTests
{
    private readonly Mock<ITopicsRepository> _topicsRepositoryMock = new();
    private readonly Mock<IIntegrationEventPublisher> _eventPublisherMock = new();
    private readonly Mock<ILogger<PresentationTopicsAnalysisService>> _loggerMock = new();

    [Fact]
    public void Constructor_NullTopicsRepository_ThrowsArgumentNullException()
    {
        var kernel = Kernel.CreateBuilder().Build();

        Assert.Throws<ArgumentNullException>(() =>
            new PresentationTopicsAnalysisService(null!, _eventPublisherMock.Object, kernel, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullEventPublisher_ThrowsArgumentNullException()
    {
        var kernel = Kernel.CreateBuilder().Build();

        Assert.Throws<ArgumentNullException>(() =>
            new PresentationTopicsAnalysisService(_topicsRepositoryMock.Object, null!, kernel, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullKernel_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PresentationTopicsAnalysisService(_topicsRepositoryMock.Object, _eventPublisherMock.Object, null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var kernel = Kernel.CreateBuilder().Build();

        Assert.Throws<ArgumentNullException>(() =>
            new PresentationTopicsAnalysisService(_topicsRepositoryMock.Object, _eventPublisherMock.Object, kernel, null!));
    }

    [Fact]
    public async Task AnalyzeAsync_NullPresentation_ThrowsArgumentNullException()
    {
        var kernel = Kernel.CreateBuilder().Build();
        var service = new PresentationTopicsAnalysisService(
            _topicsRepositoryMock.Object,
            _eventPublisherMock.Object,
            kernel,
            _loggerMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.AnalyzeAsync(Guid.NewGuid(), null!, CancellationToken.None));
    }
}
