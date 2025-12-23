using Dapr.Client;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.IntegrationEvents.Events;
using Microsoft.Extensions.Options;

namespace HexMaster.Attendr.IntegrationEvents.Services;

public sealed class IntegrationEventPublisher(DaprClient daprClient, IOptions<DaprOptions> daprOptions) : IIntegrationEventPublisher
{
    private readonly string _pubSubName = daprOptions.Value.PubSubName;

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
