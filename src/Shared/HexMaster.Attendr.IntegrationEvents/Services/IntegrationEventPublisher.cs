using Dapr.Client;
using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.IntegrationEvents.Services;

public sealed class IntegrationEventPublisher(DaprClient daprClient) : IIntegrationEventPublisher
{
    private readonly string _pubSubName = AspireConstants.Dapr.PubSubName;

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        var topicName = @event.EventType;

        await daprClient.PublishEventAsync(
            _pubSubName,
            topicName,
            @event,
            cancellationToken).ConfigureAwait(false);
    }
}
