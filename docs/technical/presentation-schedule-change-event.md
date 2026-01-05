# PresentationScheduleChangeEvent

## Overview

The `PresentationScheduleChangeEvent` is an integration event that is published when a presentation's schedule changes **and** a profile has favorited that presentation. This event is specifically designed to trigger notifications to attendees about schedule changes for presentations they care about.

## Event Type

```
presentation.schedule-changed
```

## When is it Published?

The event is published when **all** of the following conditions are met:

1. **Presentation Updated**: A presentation is updated during conference synchronization (Sessionize)
2. **Schedule Changed**: The presentation's schedule has changed (start/end time or room)
3. **Is Favorited**: The profile has marked the presentation as a favorite (`IsFavorite == true`)

The event is published **per profile** who has favorited the presentation, allowing personalized notifications.

## Event Properties

| Property | Type | Description |
|----------|------|-------------|
| `EventId` | `Guid` | Unique identifier for the event (auto-generated) |
| `OccurredAt` | `DateTime` | UTC timestamp when the event occurred (auto-generated) |
| `EventType` | `string` | Always "presentation.schedule-changed" |
| `ConferenceId` | `Guid` | The ID of the conference this presentation belongs to |
| `PresentationId` | `Guid` | The unique identifier of the presentation |
| `ProfileId` | `Guid` | The ID of the profile who has favorited this presentation |
| `Title` | `string` | The title of the presentation |
| `Abstract` | `string` | The abstract/description of the presentation |
| `Room` | `string` | The name of the room where the presentation is held |
| `StartDateTime` | `DateTime` | The (new) start date and time of the presentation |
| `EndDateTime` | `DateTime` | The (new) end date and time of the presentation |

## Publishing Logic

The event is published by the **Presence Service** in response to `PresentationUpdatedEvent`:

1. Presence service receives `PresentationUpdatedEvent`
2. Finds all `PresentationPresence` records for the conference/presentation
3. For each presence record:
   - Updates the presentation information
   - If `IsScheduleChanged == true` AND `IsFavorite == true`:
     - Publishes `PresentationScheduleChangeEvent` with the profile's ID

## Example Usage

### Publishing the Event (Presence Service)

```csharp
// In UpdatePresentationService.HandlePresentationUpdatedAsync
if (@event.IsScheduleChanged && presentation.IsFavorite)
{
    var scheduleChangeEvent = new PresentationScheduleChangeEvent
    {
        ConferenceId = @event.ConferenceId,
        PresentationId = @event.PresentationId,
        ProfileId = presentation.ProfileId,
        Title = @event.Title,
        Abstract = @event.Abstract,
        Room = @event.RoomName,
        StartDateTime = @event.StartDateTime,
        EndDateTime = @event.EndDateTime
    };

    await _eventPublisher.PublishAsync(scheduleChangeEvent, cancellationToken);
}
```

### Consuming the Event

Services that want to react to schedule changes for favorited presentations should subscribe to this event. The primary use case is sending notifications to attendees.

Example consumer (Notification Service):

```csharp
[HttpPost("/api/events/presentation-schedule-changed")]
public async Task<IActionResult> HandlePresentationScheduleChanged(
    [FromBody] PresentationScheduleChangeEvent @event,
    CancellationToken cancellationToken)
{
    _logger.LogInformation(
        "Schedule changed for favorited presentation {PresentationId} for profile {ProfileId}",
        @event.PresentationId,
        @event.ProfileId);

    // Get profile notification preferences
    var profile = await _profileService.GetAsync(@event.ProfileId, cancellationToken);
    
    if (profile.NotificationsEnabled)
    {
        // Send push notification
        await _pushNotificationService.SendAsync(
            @event.ProfileId,
            "Schedule Change",
            $"The time or location for '{@event.Title}' has changed. " +
            $"New time: {FormatTime(@event.StartDateTime)} in {(@event.Room)}.",
            cancellationToken);

        // Send email notification
        await _emailService.SendScheduleChangeEmailAsync(
            profile.Email,
            @event,
            cancellationToken);
    }

    return Ok();
}
```

## Event Flow

```
Sessionize Sync
     ↓
PresentationUpdatedEvent (Conferences Service)
     ↓
Presence Service Receives Event
     ↓
Update PresentationPresence Records
     ↓
For Each Favorited Presentation (IsFavorite == true)
     ↓
PresentationScheduleChangeEvent (per Profile)
     ↓
Notification Service
     ↓
Send Notifications to Attendees
```

## Related Events

- **PresentationUpdatedEvent**: The upstream event that triggers this event
- **ConferenceUpdatedEvent**: Published when conference-level details are updated

## Implementation Details

### Source

- **Event Definition**: `PresentationScheduleChangeEvent.cs`
- **Namespace**: `HexMaster.Attendr.IntegrationEvents.Events`
- **Publisher**: `UpdatePresentationService` in the Presence.Api module

### Event Publishing Logic

The Presence service:
1. Receives `PresentationUpdatedEvent` from Conferences service
2. Queries all `PresentationPresence` records matching `ConferenceId` + `PresentationId`
3. Updates each record with new presentation details
4. For presentations where `IsScheduleChanged == true` AND `IsFavorite == true`:
   - Publishes one `PresentationScheduleChangeEvent` per profile

### Data Preservation

When updating `PresentationPresence` records, the following fields are **preserved**:
- `IsFavorite` - Whether the profile has favorited the presentation
- `IsCheckedIn` - Whether the profile has checked into the presentation
- `IsRated` - Whether the profile has rated the presentation
- `Rating` - The rating value (if rated)

## Best Practices

1. **Idempotency**: Notification services should track sent notifications to avoid duplicates
2. **User Preferences**: Always respect user notification preferences (email, push, SMS)
3. **Rate Limiting**: Implement rate limiting to avoid notification spam during bulk updates
4. **Batching**: Consider batching notifications if multiple presentations change at once
5. **Personalization**: Include profile-specific context (e.g., "Your favorite session has moved")
6. **Actionable Content**: Include deep links to the updated presentation in the conference app

## Example Notification Messages

### Push Notification
**Title**: "Schedule Change"  
**Body**: "The time or location for 'Building Microservices with .NET' has changed. New time: Jan 15, 10:00 AM in Room A."

### Email Notification
**Subject**: "Schedule Update - Building Microservices with .NET"  
**Body**:
```
Hi [Name],

One of your favorite sessions at [Conference Name] has a schedule change:

Session: Building Microservices with .NET
New Time: January 15, 2026 at 10:00 AM
New Location: Room A

[View Updated Schedule]

Stay tuned for more updates!
```

## See Also

- [PresentationUpdatedEvent Documentation](presentation-updated-event.md)
- [Integration Events Documentation](integration-events.md)
- [Presence Service Overview](../components/presence-service.md)
