# PresentationUpdatedEvent

## Overview

The `PresentationUpdatedEvent` is an integration event that is published when a presentation is updated during conference data synchronization from external sources (e.g., Sessionize). This event is raised per presentation when at least one field has been modified.

## Event Type

```
presentation.updated
```

## When is it Published?

The event is published in the following scenarios:

1. **During Sessionize Synchronization**: When conference data is imported/synchronized from Sessionize
2. **Only When Modified**: The event is only published when at least one field of the presentation has actually changed
3. **After Persistence**: Events are published after the conference (and its presentations) has been successfully saved to the database
4. **Per Presentation**: One event is published for each updated presentation

## Event Properties

| Property | Type | Description |
|----------|------|-------------|
| `EventId` | `Guid` | Unique identifier for the event (auto-generated) |
| `OccurredAt` | `DateTime` | UTC timestamp when the event occurred (auto-generated) |
| `EventType` | `string` | Always "presentation.updated" |
| `ConferenceId` | `Guid` | The ID of the conference this presentation belongs to |
| `PresentationId` | `Guid` | The unique identifier of the presentation |
| `Title` | `string` | The title of the presentation |
| `Abstract` | `string` | The abstract/description of the presentation |
| `StartDateTime` | `DateTime` | The start date and time of the presentation |
| `EndDateTime` | `DateTime` | The end date and time of the presentation |
| `RoomId` | `Guid` | The ID of the room where the presentation is held |
| `SpeakerIds` | `IReadOnlyCollection<Guid>` | Collection of speaker IDs for this presentation |
| `ExternalId` | `string?` | The external ID from the synchronization source (e.g., Sessionize session ID) |
| `IsScheduleChanged` | `bool` | Indicates whether the schedule has changed |

## IsScheduleChanged Flag

The `IsScheduleChanged` property is set to `true` when:

- The presentation's start date/time has changed, OR
- The presentation's end date/time has changed, OR
- The presentation has been moved to a different room

This flag is useful for consumers who need to specifically handle schedule changes (e.g., sending notifications to attendees who favorited the presentation).

## Example Usage

### Publishing the Event (Conferences Service)

```csharp
var integrationEvent = new PresentationUpdatedEvent
{
    ConferenceId = conference.Id,
    PresentationId = presentation.Id,
    Title = presentation.Title,
    Abstract = presentation.Abstract,
    StartDateTime = presentation.StartDateTime,
    EndDateTime = presentation.EndDateTime,
    RoomId = presentation.RoomId,
    SpeakerIds = presentation.SpeakerIds.ToList(),
    ExternalId = presentation.ExternalId,
    IsScheduleChanged = isScheduleChanged
};

await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);
```

### Consuming the Event

Services that want to react to presentation updates should subscribe to this event. Common use cases include:

1. **Presence Service**: Update cached presentation data for profiles that favorited the presentation
2. **Notification Service**: Send notifications to attendees when:
   - The presentation schedule changes (`IsScheduleChanged == true`)
   - Speaker lineup changes
   - Presentation details are updated

Example consumer:

```csharp
[HttpPost("/api/events/presentation-updated")]
public async Task<IActionResult> HandlePresentationUpdated(
    [FromBody] PresentationUpdatedEvent @event,
    CancellationToken cancellationToken)
{
    _logger.LogInformation(
        "Received PresentationUpdatedEvent for {PresentationId} - {Title}, ScheduleChanged: {IsScheduleChanged}",
        @event.PresentationId,
        @event.Title,
        @event.IsScheduleChanged);

    // Handle schedule changes
    if (@event.IsScheduleChanged)
    {
        // Send notifications to users who favorited this presentation
        await _notificationService.NotifyScheduleChangeAsync(@event, cancellationToken);
    }

    // Update cached data
    await _cacheService.UpdatePresentationAsync(@event, cancellationToken);

    return Ok();
}
```

## Related Events

- **ConferenceCreatedEvent**: Published when a new conference is created
- **ConferenceUpdatedEvent**: Published when conference-level details are updated

## Implementation Details

### Source

- **File**: `PresentationUpdatedEvent.cs`
- **Namespace**: `HexMaster.Attendr.IntegrationEvents.Events`
- **Publisher**: `SessionizeSyncService` in the Conferences domain

### Event Publishing Logic

The event is published in the `SessionizeSyncService.SynchronizeConferenceAsync` method:

1. Track presentations that are updated during synchronization
2. Determine if schedule changed by comparing old vs. new datetime and room values
3. After saving the conference to the database, publish events for all updated presentations
4. Only presentations with `State == Modified` trigger events

### Filtering Logic

```csharp
var isScheduleChanged = 
    existingPresentation.StartDateTime != startDateTime ||
    existingPresentation.EndDateTime != endDateTime ||
    existingPresentation.RoomId != roomLocalId;
```

## Best Practices

1. **Idempotency**: Consumers should handle duplicate events gracefully
2. **Schedule Change Focus**: Use the `IsScheduleChanged` flag to prioritize critical updates
3. **Logging**: Always log event receipt with key identifiers for troubleshooting
4. **Error Handling**: Implement retry logic for failed event processing
5. **Async Processing**: Process events asynchronously to avoid blocking the publisher

## See Also

- [Integration Events Documentation](integration-events.md)
- [Sessionize Synchronization](../components/sessionize-sync.md)
- [Event Publishing Service](integration-events.md#event-publisher-service)
