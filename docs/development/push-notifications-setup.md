# Push Notifications Configuration Guide

## Overview

The Attendr PWA is configured to support Web Push Notifications. This guide explains how to set up and use push notifications in your application.

## Prerequisites

1. **HTTPS**: Push notifications only work over HTTPS (except localhost for development)
2. **Service Worker**: Already enabled in the Angular app configuration
3. **VAPID Keys**: You need to generate a public/private key pair for Web Push

## Step 1: Generate VAPID Keys

VAPID (Voluntary Application Server Identification) keys are required to send push notifications. Generate them using a tool or library:

### Using Node.js (web-push library)
```bash
npm install -g web-push

# Generate keys
web-push generate-vapid-keys

# Output:
# Public Key: <your-public-key>
# Private Key: <your-private-key>
```

### Using an Online Tool
Visit https://tools.reactpwa.com/vapid - this generates the keys in your browser

## Step 2: Configure the Frontend

### Add VAPID Public Key to Environment

Update your environment configuration:

```typescript
// environments/environment.ts
export const environment = {
  apiUrl: 'https://api.attendr.live',
  vapidPublicKey: '<your-vapid-public-key>'  // Add this
};
```

### Inject and Use the Push Notification Service

```typescript
import { Component, inject } from '@angular/core';
import { PushNotificationService } from '@shared/services/push-notification.service';

@Component({
  selector: 'app-notification-settings',
  template: `
    <button (click)="subscribeToPushNotifications()" 
            [disabled]="!pushService.isSupported()">
      Enable Push Notifications
    </button>
    <p *ngIf="pushService.isSubscribed()">
      Push notifications are enabled
    </p>
  `
})
export class NotificationSettingsComponent {
  pushService = inject(PushNotificationService);

  async subscribeToPushNotifications() {
    try {
      const subscription = await this.pushService.subscribe(
        environment.vapidPublicKey
      );

      // Send subscription to your backend
      await this.sendSubscriptionToServer(subscription);
    } catch (error) {
      console.error('Failed to subscribe to push notifications:', error);
    }
  }

  private async sendSubscriptionToServer(subscription: PushSubscription) {
    // TODO: Send subscription endpoint and keys to your backend
    // POST to your notifications API
  }
}
```

## Step 3: Configure the Backend

### Store Subscription Data

When a user subscribes, send the subscription data to your backend:

```typescript
const subscriptionData = {
  endpoint: subscription.endpoint,
  keys: {
    p256dh: 'base64-encoded-p256dh-key',
    auth: 'base64-encoded-auth-key'
  },
  profileId: 'user-profile-id'
};

// POST to /api/notifications/subscriptions
```

### Send Push Notifications

Use the Web Push library on your backend to send notifications:

**.NET Example:**

```csharp
// Install NuGet package: WebPush
using WebPush;

public class PushNotificationService
{
    private readonly string _vapidPublicKey;
    private readonly string _vapidPrivateKey;

    public async Task SendPushNotificationAsync(
        string endpoint,
        string p256dhKey,
        string authKey,
        string payload)
    {
        var pushClient = new WebPushClient();
        var subscription = new PushSubscription(endpoint, p256dhKey, authKey);
        
        try
        {
            await pushClient.SendNotificationAsync(
                subscription,
                payload,
                new VapidDetails(_vapidPublicKey, _vapidPrivateKey)
            );
        }
        catch (HttpRequestException ex)
        {
            // Handle subscription expired or invalid
            // Delete from database if endpoint is invalid
        }
    }
}
```

**Payload Format:**

```json
{
  "title": "Conference Updated",
  "body": "The conference schedule has been updated",
  "icon": "https://attendr.live/images/attendr-icon.png",
  "badge": "https://attendr.live/images/badge.png",
  "tag": "conference-update",
  "requireInteraction": false,
  "url": "https://attendr.live/app/conferences/123",
  "actions": [
    {
      "action": "view",
      "title": "View",
      "url": "https://attendr.live/app/conferences/123"
    },
    {
      "action": "dismiss",
      "title": "Dismiss"
    }
  ]
}
```

## Step 4: Integration with Notification Service

You can integrate push notifications with your existing notification system:

### Trigger Push on Notification Events

In your notification event handlers, check if a user has push subscriptions:

```csharp
public async Task HandleGroupAccessRequestedAsync(GroupAccessRequestedEvent @event)
{
    var subscribers = await _database
        .PushSubscriptions
        .Where(s => s.ProfileId == @event.AdminProfileId)
        .ToListAsync();

    foreach (var subscriber in subscribers)
    {
        var payload = JsonSerializer.Serialize(new
        {
            title = "Group Access Request",
            body = $"{@event.ProfileName} requested access to {@event.GroupName}",
            url = $"/app/groups/{@event.GroupId}",
            data = new { groupId = @event.GroupId }
        });

        await _pushService.SendPushNotificationAsync(
            subscriber.Endpoint,
            subscriber.P256dhKey,
            subscriber.AuthKey,
            payload
        );
    }
}
```

## Service Worker Push Event Handling

The service worker automatically:
1. Receives push events from the browser
2. Parses JSON payload
3. Shows a notification to the user
4. Handles notification clicks to navigate to relevant page
5. Supports action buttons

## Testing Push Notifications

### Chrome DevTools
1. Open DevTools → Application tab
2. Go to Service Workers section
3. Enable "Offline" to test
4. Use the "Push" input to send test notifications

### Manual Testing
```javascript
// In browser console (when subscribed)
const registration = await navigator.serviceWorker.ready;
const subscription = await registration.pushManager.getSubscription();

// Send test push via backend API
fetch('/api/notifications/test-push', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    title: 'Test Notification',
    body: 'This is a test'
  })
});
```

## Manifest.webmanifest

The PWA manifest is already configured to support push notifications. The key property is `display: "standalone"` which is required for push notifications to work properly.

## Best Practices

1. **Request permission at the right time**: Don't ask for permission on first load. Ask when showing a benefit.
2. **Handle permission denial gracefully**: Don't force users to enable notifications.
3. **Keep subscriptions valid**: Monitor for expired subscriptions and clean them up.
4. **Test on real devices**: PWAs and push work best on actual mobile devices.
5. **Use meaningful titles and bodies**: Keep messages concise and actionable.
6. **Validate subscriptions**: Periodically check if subscriptions are still valid.

## Troubleshooting

### Push notifications not working
- Ensure you're using HTTPS (or localhost with 127.0.0.1)
- Check browser console for errors
- Verify service worker is registered and active
- Check notification permission is set to "granted"

### User hasn't granted permission
- The service shows an error
- Call `requestPermission()` again when user clicks enable button
- Check browser notification settings

### Subscription expired
- Delete expired subscriptions from database when push fails
- Resubscribe user on next login

## Files Modified/Created

- `src/app/shared/services/push-notification.service.ts` - Push subscription service
- `src/app/shared/service-worker.ts` - Service worker push event handling
- `public/manifest.webmanifest` - PWA manifest with push support
- `src/main.ts` - Already configured with service worker
- `ngsw-config.json` - Service worker configuration
