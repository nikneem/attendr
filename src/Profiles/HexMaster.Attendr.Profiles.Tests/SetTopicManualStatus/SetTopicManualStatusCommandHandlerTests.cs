using Bogus;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Features.SetTopicManualStatus;
using HexMaster.Attendr.Profiles.Observability;
using HexMaster.Attendr.Profiles.Repositories;
using HexMaster.Attendr.Profiles.Services;
using HexMaster.Attendr.Profiles.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Profiles.Tests.SetTopicManualStatus;

public class SetTopicManualStatusCommandHandlerTests
{
    private readonly Faker _faker = new();
    private readonly Mock<IProfileTopicRepository> _repository;
    private readonly Mock<IIntegrationEventPublisher> _eventPublisher;
    private readonly TopicWeightDecayService _decayService;
    private readonly ProfileMetrics _metrics;
    private readonly Mock<ILogger<SetTopicManualStatusCommandHandler>> _logger;
    private readonly SetTopicManualStatusCommandHandler _handler;

    public SetTopicManualStatusCommandHandlerTests()
    {
        _repository = new Mock<IProfileTopicRepository>();
        _eventPublisher = new Mock<IIntegrationEventPublisher>();
        _decayService = new TopicWeightDecayService();
        _metrics = TestMetricsFactory.CreateProfileMetrics();
        _logger = new Mock<ILogger<SetTopicManualStatusCommandHandler>>();
        _handler = new SetTopicManualStatusCommandHandler(
            _repository.Object,
            _eventPublisher.Object,
            _decayService,
            _metrics,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ShouldSetIsManualToTrue_WhenTopicExists()
    {
        var topicId = _faker.Random.Guid().ToString();
        var profileId = _faker.Random.Guid().ToString();
        var topic = ProfileTopic.FromPersisted(
            topicId,
            profileId,
            "ai",
            "Artificial Intelligence",
            false,
            new[] { new Occasion(80, DateTimeOffset.UtcNow.AddDays(-1)) },
            DateTimeOffset.UtcNow.AddDays(-2),
            null);

        _repository.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProfileTopic> { topic });

        var command = new SetTopicManualStatusCommand(topicId, true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(topicId, result.Id);
        Assert.True(result.IsManual);
        _repository.Verify(r => r.UpsertAsync(It.Is<ProfileTopic>(t => t.IsManual == true), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<ProfileTopicsChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetIsManualToFalse_WhenTopicExists()
    {
        var topicId = _faker.Random.Guid().ToString();
        var profileId = _faker.Random.Guid().ToString();
        var topic = ProfileTopic.FromPersisted(
            topicId,
            profileId,
            "ai",
            "Artificial Intelligence",
            true,
            new[] { new Occasion(100, DateTimeOffset.UtcNow) },
            DateTimeOffset.UtcNow.AddDays(-2),
            null);

        _repository.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProfileTopic> { topic });

        var command = new SetTopicManualStatusCommand(topicId, false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(topicId, result.Id);
        Assert.False(result.IsManual);
        _repository.Verify(r => r.UpsertAsync(It.Is<ProfileTopic>(t => t.IsManual == false), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<ProfileTopicsChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenTopicDoesNotExist()
    {
        var topicId = _faker.Random.Guid().ToString();

        _repository.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileTopic?)null);

        var command = new SetTopicManualStatusCommand(topicId, true);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenTopicIdIsEmpty()
    {
        var command = new SetTopicManualStatusCommand(string.Empty, true);

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectDto_WithCalculatedWeight()
    {
        var topicId = _faker.Random.Guid().ToString();
        var profileId = _faker.Random.Guid().ToString();
        var topic = ProfileTopic.FromPersisted(
            topicId,
            profileId,
            "cloudnative",
            "Cloud Native",
            false,
            new[]
            {
                new Occasion(70, DateTimeOffset.UtcNow.AddDays(-1)),
                new Occasion(85, DateTimeOffset.UtcNow.AddDays(-5))
            },
            DateTimeOffset.UtcNow.AddDays(-10),
            null);

        _repository.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProfileTopic> { topic });

        var command = new SetTopicManualStatusCommand(topicId, true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(100, result.Weight); // Manual topics always return 100
        Assert.Equal("Cloud Native", result.TopicName);
        Assert.Equal("cloudnative", result.TopicKey);
    }

    [Fact]
    public async Task Handle_ShouldReturn100Weight_WhenSettingManualToTrue()
    {
        var topicId = _faker.Random.Guid().ToString();
        var profileId = _faker.Random.Guid().ToString();
        var topic = ProfileTopic.FromPersisted(
            topicId,
            profileId,
            "ai",
            "Artificial Intelligence",
            false,
            new[] { new Occasion(50, DateTimeOffset.UtcNow.AddDays(-1)) },
            DateTimeOffset.UtcNow.AddDays(-2),
            null);

        _repository.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProfileTopic> { topic });

        var command = new SetTopicManualStatusCommand(topicId, true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(100, result.Weight); // Manual topics always return 100
        Assert.True(result.IsManual);
    }

    [Fact]
    public async Task Handle_ShouldReturnActualWeight_WhenSettingManualToFalse()
    {
        var topicId = _faker.Random.Guid().ToString();
        var profileId = _faker.Random.Guid().ToString();
        var topic = ProfileTopic.FromPersisted(
            topicId,
            profileId,
            "ai",
            "Artificial Intelligence",
            true,
            new[] { new Occasion(75, DateTimeOffset.UtcNow.AddDays(-1)) },
            DateTimeOffset.UtcNow.AddDays(-2),
            null);

        _repository.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);
        _repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProfileTopic> { topic });

        var command = new SetTopicManualStatusCommand(topicId, false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(75, result.Weight); // Non-manual topics return actual weight
        Assert.False(result.IsManual);
    }
}
