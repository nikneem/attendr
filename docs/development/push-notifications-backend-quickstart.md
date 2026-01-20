# Push Notifications - Backend Implementation Quick Start

## TL;DR - What You Need to Do

1. **Generate VAPID keys** (one-time setup)
   ```bash
   npm install -g web-push
   web-push generate-vapid-keys --json
   ```

2. **Add to `appsettings.Development.json`**
   ```json
   "PushNotifications": {
     "VapidPublicKey": "YOUR_PUBLIC_KEY",
     "VapidPrivateKey": "YOUR_PRIVATE_KEY",
     "VapidSubject": "mailto:notifications@attendr.live"
   }
   ```

3. **Install WebPush NuGet**
   ```bash
   dotnet add package WebPush
   ```

4. **Create push subscription endpoints** (POST/DELETE to store subscriptions)

5. **Wire notification events** to send push via `IPushNotificationSender`

6. **For frontend**: VAPID public key goes in `src/environments/environment.ts`

## Step-by-Step Backend Implementation

### Step 1: Generate and Configure VAPID Keys

```bash
# Install web-push CLI globally
npm install -g web-push

# Generate keys
web-push generate-vapid-keys --json > vapid-keys.json

# Output:
# {
#   "publicKey": "BCo...",
#   "privateKey": "abc..."
# }
```

Copy keys to your configuration files:

**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "PushNotifications": {
    "VapidPublicKey": "BCo...",
    "VapidPrivateKey": "abc...",
    "VapidSubject": "mailto:notifications@attendr.live"
  }
}
```

**appsettings.Production.json:** (Use secure configuration in production)
```json
{
  "PushNotifications": {
    "VapidPublicKey": "${PUSH_VAPID_PUBLIC_KEY}",
    "VapidPrivateKey": "${PUSH_VAPID_PRIVATE_KEY}",
    "VapidSubject": "mailto:notifications@attendr.live"
  }
}
```

### Step 2: Add WebPush NuGet Package

```bash
cd src/HexMaster.Attendr.Notifications.Api
dotnet add package WebPush
```

### Step 3: Create Domain Model for Push Subscriptions

**HexMaster.Attendr.Notifications/Domain/PushSubscription.cs:**
```csharp
namespace HexMaster.Attendr.Notifications.Domain;

public class PushSubscription
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid ProfileId { get; set; }
    public string Endpoint { get; set; }
    public string P256DH { get; set; }
    public string Auth { get; set; }
    public string UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Step 4: Add Repository Methods

**HexMaster.Attendr.Notifications.Abstractions/INotificationPreferencesRepository.cs:**
```csharp
public interface INotificationPreferencesRepository
{
    // ... existing methods ...
    
    Task SavePushSubscriptionAsync(PushSubscription subscription);
    Task DeletePushSubscriptionAsync(string subscriptionId);
    Task DeletePushSubscriptionByProfileAsync(Guid profileId);
    Task<PushSubscription> GetPushSubscriptionAsync(string subscriptionId);
    Task<IEnumerable<PushSubscription>> GetPushSubscriptionsForUserAsync(Guid profileId);
}
```

**HexMaster.Attendr.Notifications.Data.Postgres/NotificationPreferencesRepository.cs:**
```csharp
public async Task SavePushSubscriptionAsync(PushSubscription subscription)
{
    // Save to database - adjust based on your ORM
    _dbContext.PushSubscriptions.Add(subscription);
    await _dbContext.SaveChangesAsync();
}

public async Task DeletePushSubscriptionAsync(string subscriptionId)
{
    var subscription = await _dbContext.PushSubscriptions.FindAsync(subscriptionId);
    if (subscription != null)
    {
        _dbContext.PushSubscriptions.Remove(subscription);
        await _dbContext.SaveChangesAsync();
    }
}

public async Task<IEnumerable<PushSubscription>> GetPushSubscriptionsForUserAsync(Guid profileId)
{
    return await _dbContext.PushSubscriptions
        .Where(s => s.ProfileId == profileId && s.IsActive)
        .ToListAsync();
}
```

### Step 5: Create Push Notification Service

**HexMaster.Attendr.Notifications.Api/Services/PushNotificationSender.cs:**
```csharp
using WebPush;
using Microsoft.Extensions.Options;
using HexMaster.Attendr.Notifications.Domain;

namespace HexMaster.Attendr.Notifications.Api.Services;

public interface IPushNotificationSender
{
    Task SendAsync(string endpoint, string auth, string p256dh, PushNotificationPayload payload);
    Task SendToSubscriberAsync(PushSubscription subscription, PushNotificationPayload payload);
}

public class PushNotificationSender : IPushNotificationSender
{
    private readonly WebPushClient _webPushClient;
    private readonly PushNotificationOptions _options;
    private readonly ILogger<PushNotificationSender> _logger;

    public PushNotificationSender(
        IOptions<PushNotificationOptions> options,
        ILogger<PushNotificationSender> logger)
    {
        _options = options.Value;
        _logger = logger;
        _webPushClient = new WebPushClient();
    }

    public async Task SendAsync(string endpoint, string auth, string p256dh, PushNotificationPayload payload)
    {
        try
        {
            var vapidDetails = new VapidDetails(
                _options.VapidSubject,
                _options.VapidPublicKey,
                _options.VapidPrivateKey);

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            
            await _webPushClient.SendNotificationAsync(
                new PushSubscription(endpoint, p256dh, auth),
                json,
                vapidDetails);

            _logger.LogInformation("Push notification sent to {Endpoint}", endpoint);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
        {
            _logger.LogWarning("Push subscription expired: {Endpoint}", endpoint);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification");
            throw;
        }
    }

    public async Task SendToSubscriberAsync(PushSubscription subscription, PushNotificationPayload payload)
    {
        await SendAsync(subscription.Endpoint, subscription.Auth, subscription.P256DH, payload);
    }
}

public class PushNotificationOptions
{
    public string VapidPublicKey { get; set; }
    public string VapidPrivateKey { get; set; }
    public string VapidSubject { get; set; } = "mailto:notifications@attendr.live";
}

public class PushNotificationPayload
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string Icon { get; set; }
    public string Badge { get; set; }
    public string Tag { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
}
```

### Step 6: Register Services in Program.cs

```csharp
// Add push notification options
builder.Services.Configure<PushNotificationOptions>(
    builder.Configuration.GetSection("PushNotifications"));

// Add push notification sender
builder.Services.AddSingleton<IPushNotificationSender, PushNotificationSender>();
```

### Step 7: Create Push Subscription Endpoints

**HexMaster.Attendr.Notifications.Api/Endpoints/PushSubscriptionsEndpoints.cs:**
```csharp
using HexMaster.Attendr.Notifications.Abstractions.Domain;
using HexMaster.Attendr.Notifications.Api.Services;
using HexMaster.Attendr.Shared.Abstractions.Profiles;

namespace HexMaster.Attendr.Notifications.Api.Endpoints;

public static class PushSubscriptionsEndpoints
{
    public static void MapPushSubscriptionsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications/subscriptions")
            .WithName("PushSubscriptions")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", Subscribe)
            .WithName("SubscribeToPush")
            .WithDescription("Subscribe to push notifications");

        group.MapDelete("/{subscriptionId}", Unsubscribe)
            .WithName("UnsubscribeFromPush")
            .WithDescription("Unsubscribe from push notifications");
    }

    private static async Task<IResult> Subscribe(
        SubscribePushRequest request,
        IProfilesIntegrationService profilesService,
        INotificationPreferencesRepository preferencesRepository,
        ILogger<PushSubscriptionsEndpoints> logger)
    {
        try
        {
            var profileId = await profilesService.GetProfileIdAsync();
            
            var subscription = new PushSubscription
            {
                ProfileId = profileId,
                Endpoint = request.Endpoint,
                P256DH = request.Keys.P256DH,
                Auth = request.Keys.Auth,
                UserAgent = request.UserAgent ?? "",
                CreatedAt = DateTime.UtcNow
            };

            await preferencesRepository.SavePushSubscriptionAsync(subscription);
            
            logger.LogInformation("Push subscription created for user {ProfileId}", profileId);
            
            return Results.Created($"/api/notifications/subscriptions/{subscription.Id}", 
                new { subscriptionId = subscription.Id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error subscribing to push notifications");
            return Results.BadRequest(new { message = "Failed to subscribe" });
        }
    }

    private static async Task<IResult> Unsubscribe(
        string subscriptionId,
        IProfilesIntegrationService profilesService,
        INotificationPreferencesRepository preferencesRepository,
        ILogger<PushSubscriptionsEndpoints> logger)
    {
        try
        {
            var profileId = await profilesService.GetProfileIdAsync();
            
            // Verify ownership before deleting
            var subscription = await preferencesRepository.GetPushSubscriptionAsync(subscriptionId);
            if (subscription?.ProfileId != profileId)
            {
                return Results.Forbid();
            }

            await preferencesRepository.DeletePushSubscriptionAsync(subscriptionId);
            
            logger.LogInformation("Push subscription deleted for user {ProfileId}", profileId);
            
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unsubscribing from push notifications");
            return Results.BadRequest(new { message = "Failed to unsubscribe" });
        }
    }
}

public class SubscribePushRequest
{
    public string Endpoint { get; set; }
    public string UserAgent { get; set; }
    public PushSubscriptionKeys Keys { get; set; }
}

public class PushSubscriptionKeys
{
    public string P256DH { get; set; }
    public string Auth { get; set; }
}
```

### Step 8: Register Endpoints in Program.cs

```csharp
// Map all notification endpoints
app.MapNotificationsEndpoints();
app.MapNotificationPreferencesEndpoints();
app.MapNotificationPreferencesDetailEndpoints();
app.MapPushSubscriptionsEndpoints();  // Add this line
```

### Step 9: Wire Notification Events

In your event handlers, send push notifications to subscribed users:

```csharp
public class GroupAccessRequestedEventHandler 
    : INotificationEventHandler<GroupAccessRequestedEvent>
{
    private readonly INotificationPreferencesRepository _preferencesRepository;
    private readonly IPushNotificationSender _pushSender;
    private readonly ILogger<GroupAccessRequestedEventHandler> _logger;

    public async Task HandleAsync(GroupAccessRequestedEvent notification)
    {
        try
        {
            // Get all push subscriptions for the group owner
            var subscriptions = await _preferencesRepository
                .GetPushSubscriptionsForUserAsync(notification.GroupOwnerId);

            if (!subscriptions.Any())
            {
                _logger.LogDebug("No push subscriptions for user {UserId}", 
                    notification.GroupOwnerId);
                return;
            }

            var payload = new PushNotificationPayload
            {
                Title = "Group Access Request",
                Body = $"{notification.RequesterName} requested to join {notification.GroupName}",
                Icon = "/images/attendr-icon.png",
                Badge = "/images/attendr-badge.png",
                Tag = $"group-request-{notification.GroupId}",
                Data = new Dictionary<string, string>
                {
                    { "url", $"/app/groups/{notification.GroupId}/members/requests" },
                    { "groupId", notification.GroupId.ToString() }
                }
            };

            foreach (var subscription in subscriptions)
            {
                try
                {
                    await _pushSender.SendToSubscriberAsync(subscription, payload);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
                {
                    // Subscription expired, delete it
                    _logger.LogWarning("Deleting expired push subscription {SubscriptionId}", 
                        subscription.Id);
                    await _preferencesRepository.DeletePushSubscriptionAsync(subscription.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send push to subscription {SubscriptionId}", 
                        subscription.Id);
                    // Continue to next subscription
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GroupAccessRequestedEvent");
        }
    }
}
```

## Testing

### Test Push with PowerShell

```powershell
# Set your values
$endpoint = "https://fcm.googleapis.com/fcm/send/..."
$p256dh = "..."
$auth = "..."

# From cmd/powershell:
dotnet add package WebPush
```

### Test via API

```bash
curl -X POST http://localhost:5001/api/notifications/subscriptions \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "endpoint": "https://fcm.googleapis.com/fcm/send/...",
    "userAgent": "Mozilla/5.0...",
    "keys": {
      "p256dh": "...",
      "auth": "..."
    }
  }'
```

## Next Steps

1. ✅ Generate VAPID keys
2. ✅ Configure both frontend and backend
3. ✅ Implement backend push endpoints
4. ✅ Test subscription storage
5. ✅ Wire notification events
6. ✅ Test end-to-end on mobile device
7. ✅ Deploy with secure key management

See [push-notifications-setup.md](./push-notifications-setup.md) for complete documentation.
