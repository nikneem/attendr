using HexMaster.Attendr.Conferences.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.Services;

public class PresentationAnalysisBackgroundServiceTests
{
    [Fact]
    public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
    {
        var logger = new Mock<ILogger<PresentationAnalysisBackgroundService>>();

        Assert.Throws<ArgumentNullException>(() =>
            new PresentationAnalysisBackgroundService(null!, logger.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var serviceProvider = new Mock<IServiceProvider>();

        Assert.Throws<ArgumentNullException>(() =>
            new PresentationAnalysisBackgroundService(serviceProvider.Object, null!));
    }

    [Fact]
    public void Constructor_ValidArgs_CreatesInstance()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<PresentationAnalysisBackgroundService>>();

        var service = new PresentationAnalysisBackgroundService(serviceProvider.Object, logger.Object);

        Assert.NotNull(service);
    }

    [Fact]
    public async Task StartAsync_ThenStopAsync_LogsStartedMessage()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<PresentationAnalysisBackgroundService>>();
        var service = new PresentationAnalysisBackgroundService(serviceProvider.Object, logger.Object);

        // Start the background service, which begins ExecuteAsync
        await service.StartAsync(CancellationToken.None);

        // Immediately stop - this cancels the stoppingToken, causing the 30s initial delay to terminate
        await service.StopAsync(CancellationToken.None);

        service.Dispose();

        // Verify the service logged it started
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
