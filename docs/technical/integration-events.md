# HexMaster.Attendr.IntegrationEvents

This library provides integration event publishing capabilities for the Attendr system using Dapr pub/sub.

## Features

- **Event Base Class**: All integration events inherit from `IntegrationEvent` with automatic event ID and timestamp generation
- **Event Publisher Service**: Simple service interface for publishing events via Dapr
- **Configuration Support**: Uses the options pattern to configure Dapr pub/sub component name
- **Dependency Injection**: Easy setup with extension methods

## Installation

Add a reference to this project and configure it in your `Program.cs`:

```csharp
using HexMaster.Attendr.IntegrationEvents.Extensions;

builder.Services.AddIntegrationEvents(builder.Configuration);
```

## Configuration

Add the following section to your `appsettings.json`:

```json
{
  "Dapr": {
    "PubSubName": "pubsub"
  }
}
```

## Usage

### 1. Define Your Event

Create a new event class inheriting from `IntegrationEvent`:

```csharp
using HexMaster.Attendr.IntegrationEvents.Events;

public sealed class UserRegisteredEvent : IntegrationEvent
{
    public override string EventType => "user.registered";
    
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
```

### 2. Publish the Event

Inject `IIntegrationEventPublisher` and publish your event:

```csharp
using HexMaster.Attendr.IntegrationEvents.Services;

public class UserService
{
    private readonly IIntegrationEventPublisher _eventPublisher;
    
    public UserService(IIntegrationEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    
    public async Task RegisterUserAsync(string userId, string email, CancellationToken cancellationToken)
    {
        // ... user registration logic ...
        
        var @event = new UserRegisteredEvent
        {
            UserId = userId,
            Email = email
        };
        
        await _eventPublisher.PublishAsync(@event, cancellationToken);
    }
}
```

## Included Events

The library includes several pre-defined events:

- **ProfileCreatedEvent**: Published when a user profile is created
- **ProfileUpdatedEvent**: Published when a user profile is updated
- **ConferenceCreatedEvent**: Published when a conference is created
- **ConferenceUpdatedEvent**: Published when a conference is updated

## Event Format

Each event automatically includes:

- `EventId`: Unique identifier for the event (Guid)
- `OccurredAt`: Timestamp when the event occurred (UTC)
- `EventType`: String identifier used as the Dapr topic name

## Integration with Dapr

Events are published using Dapr's pub/sub component. The `EventType` property is used as the topic name, allowing services to subscribe to specific event types through Dapr subscriptions.

Example Dapr subscription:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Subscription
metadata:
  name: profile-created-subscription
spec:
  topic: profile.created
  route: /api/events/profile-created
  pubsubname: pubsub
```
