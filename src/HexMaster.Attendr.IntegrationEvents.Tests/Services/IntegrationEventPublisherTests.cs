using Dapr.Client;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.IntegrationEvents.Services;
using Moq;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Services;

public class IntegrationEventPublisherTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly IntegrationEventPublisher _publisher;

    public IntegrationEventPublisherTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _publisher = new IntegrationEventPublisher(_daprClientMock.Object);
    }

    [Fact]
    public async Task PublishAsync_NullEvent_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _publisher.PublishAsync<ConferenceCreatedEvent>(null!, CancellationToken.None));
    }

    [Fact]
    public async Task PublishAsync_ValidEvent_CallsDaprPublishEventAsync()
    {
        var evt = new ConferenceCreatedEvent
        {
            ConferenceId = Guid.NewGuid(),
            Title = "Test Conference",
            City = "Amsterdam",
            Country = "Netherlands",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        };

        _daprClientMock
            .Setup(d => d.PublishEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ConferenceCreatedEvent>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _publisher.PublishAsync(evt, CancellationToken.None);

        _daprClientMock.Verify(d => d.PublishEventAsync(
            It.IsAny<string>(),
            evt.EventType,
            evt,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_UsesEventTypeAsTopic()
    {
        var capturedTopic = string.Empty;
        var evt = new ProfileCreatedEvent
        {
            ProfileId = "profile-1",
            SubjectId = "sub-1",
            DisplayName = "Test User"
        };

        _daprClientMock
            .Setup(d => d.PublishEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ProfileCreatedEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, ProfileCreatedEvent, CancellationToken>(
                (_, topic, _, _) => capturedTopic = topic)
            .Returns(Task.CompletedTask);

        await _publisher.PublishAsync(evt, CancellationToken.None);

        Assert.Equal(evt.EventType, capturedTopic);
        Assert.Equal("profile.created", capturedTopic);
    }

    [Fact]
    public async Task PublishAsync_UsesPubSubName_AttendRPubSub()
    {
        var capturedPubSubName = string.Empty;
        var evt = new ConferenceCreatedEvent();

        _daprClientMock
            .Setup(d => d.PublishEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ConferenceCreatedEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, ConferenceCreatedEvent, CancellationToken>(
                (pubSub, _, _, _) => capturedPubSubName = pubSub)
            .Returns(Task.CompletedTask);

        await _publisher.PublishAsync(evt, CancellationToken.None);

        Assert.Equal("attendr-pubsub", capturedPubSubName);
    }

    [Fact]
    public async Task PublishAsync_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        var evt = new ConferenceCreatedEvent();

        _daprClientMock
            .Setup(d => d.PublishEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ConferenceCreatedEvent>(),
                cts.Token))
            .Returns(Task.CompletedTask);

        await _publisher.PublishAsync(evt, cts.Token);

        _daprClientMock.Verify(d => d.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<ConferenceCreatedEvent>(),
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_UseCorrectTopics()
    {
        var topicsUsed = new List<string>();

        _daprClientMock
            .Setup(d => d.PublishEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IntegrationEvent>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, IntegrationEvent, CancellationToken>(
                (_, topic, _, _) => topicsUsed.Add(topic))
            .Returns(Task.CompletedTask);

        var conferenceEvt = new ConferenceCreatedEvent();
        var profileEvt = new ProfileCreatedEvent { ProfileId = "p1", SubjectId = "s1", DisplayName = "D" };

        await _publisher.PublishAsync(conferenceEvt, CancellationToken.None);
        await _publisher.PublishAsync(profileEvt, CancellationToken.None);

        Assert.Contains("conference.created", topicsUsed);
        Assert.Contains("profile.created", topicsUsed);
    }
}
