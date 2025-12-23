using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.IntegrationEvents.Services;

public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes an integration event to the event bus via Dapr pub/sub.
    /// </summary>
    /// <typeparam name="TEvent">The type of integration event to publish.</typeparam>
    /// <param name="event">The event instance to publish.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent;
}
