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

}
