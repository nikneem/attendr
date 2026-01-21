using Bogus;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Plugins;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.Plugins;

/// <summary>
/// Unit tests for the TopicsPlugin Semantic Kernel plugin.
/// </summary>
public class TopicsPluginTests
{
    private readonly Mock<ITopicsRepository> _mockTopicsRepository;
    private readonly Mock<ILogger<TopicsPlugin>> _mockLogger;
    private readonly TopicsPlugin _plugin;
    private readonly Faker _faker;

    public TopicsPluginTests()
    {
        _mockTopicsRepository = new Mock<ITopicsRepository>();
        _mockLogger = new Mock<ILogger<TopicsPlugin>>();
        _plugin = new TopicsPlugin(_mockTopicsRepository.Object, _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task GetExistingTopicsAsync_ReturnsFormattedListWhenTopicsExist()
    {
        // Arrange
        var topics = new List<Topic>
        {
            Topic.FromPersisted(Guid.NewGuid(), "azure", "Azure", true, DateTime.UtcNow),
            Topic.FromPersisted(Guid.NewGuid(), "dotnet", ".NET", true, DateTime.UtcNow),
            Topic.FromPersisted(Guid.NewGuid(), "kubernetes", "Kubernetes", true, DateTime.UtcNow)
        };

        _mockTopicsRepository
            .Setup(r => r.ListTopicsAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topics);

        // Act
        var result = await _plugin.GetExistingTopicsAsync();

        // Assert
        Assert.Contains(".NET", result);
        Assert.Contains("Azure", result);
        Assert.Contains("Kubernetes", result);
        Assert.Contains("Existing topics (3)", result);
        Assert.Contains("prefer to reuse these topics if semantically similar", result);

        _mockTopicsRepository.Verify(
            r => r.ListTopicsAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetExistingTopicsAsync_ReturnsNoTopicsMessageWhenNoneExist()
    {
        // Arrange
        _mockTopicsRepository
            .Setup(r => r.ListTopicsAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Topic>());

        // Act
        var result = await _plugin.GetExistingTopicsAsync();

        // Assert
        Assert.Contains("No existing topics available", result);
        Assert.Contains("You can create new topics freely", result);

        _mockTopicsRepository.Verify(
            r => r.ListTopicsAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetExistingTopicsAsync_ReturnsGracefulMessageOnException()
    {
        // Arrange
        _mockTopicsRepository
            .Setup(r => r.ListTopicsAsync(false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _plugin.GetExistingTopicsAsync();

        // Assert
        Assert.Contains("Unable to fetch existing topics", result);
        Assert.Contains("You may proceed with topic creation as needed", result);

        _mockTopicsRepository.Verify(
            r => r.ListTopicsAsync(false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetExistingTopicsAsync_SortsTopicsAlphabetically()
    {
        // Arrange
        var topics = new List<Topic>
        {
            Topic.FromPersisted(Guid.NewGuid(), "zulu", "Zulu", true, DateTime.UtcNow),
            Topic.FromPersisted(Guid.NewGuid(), "alpha", "Alpha", true, DateTime.UtcNow),
            Topic.FromPersisted(Guid.NewGuid(), "bravo", "Bravo", true, DateTime.UtcNow)
        };

        _mockTopicsRepository
            .Setup(r => r.ListTopicsAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topics);

        // Act
        var result = await _plugin.GetExistingTopicsAsync();

        // Assert
        var alphaIndex = result.IndexOf("Alpha", StringComparison.Ordinal);
        var bravoIndex = result.IndexOf("Bravo", StringComparison.Ordinal);
        var zuluIndex = result.IndexOf("Zulu", StringComparison.Ordinal);

        Assert.True(alphaIndex < bravoIndex, "Alpha should come before Bravo");
        Assert.True(bravoIndex < zuluIndex, "Bravo should come before Zulu");
    }
}
