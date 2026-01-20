# Detailed Notification Preferences Endpoints

## Overview

The detailed notification preferences endpoints provide a comprehensive way to manage user notification settings by combining:
- User's saved preferences from Table Storage
- All available notification types with their configurations
- Default channel settings for each notification type
- Channel availability constraints per notification type

## Endpoints

### GET /api/notifications/preferences/detailed

Retrieves the user's notification preferences combined with notification type configurations.

**Authentication:** Required (JWT Bearer token)

**Response: 200 OK**

```json
{
  "profileId": "550e8400-e29b-41d4-a716-446655440000",
  "updatedAt": "2024-01-15T10:30:00Z",
  "doNotDisturbUntil": "2024-01-15T18:00:00Z",
  "notificationTypes": [
    {
      "typeKey": "GroupMemberAdded",
      "displayName": "Group Member Added",
      "description": "Notification when a member is added to a group",
      "channelPreferences": {
        "InApp": {
          "channelName": "InApp",
          "isAvailable": true,
          "isEnabled": true,
          "isDefaultEnabled": true
        },
        "Email": {
          "channelName": "Email",
          "isAvailable": true,
          "isEnabled": false,
          "isDefaultEnabled": false
        },
        "Push": {
          "channelName": "Push",
          "isAvailable": true,
          "isEnabled": true,
          "isDefaultEnabled": true
        }
      }
    },
    {
      "typeKey": "GroupAccessRequested",
      "displayName": "Group Access Requested",
      "description": "Notification when someone requests access to a group",
      "channelPreferences": {
        "InApp": {
          "channelName": "InApp",
          "isAvailable": true,
          "isEnabled": true,
          "isDefaultEnabled": true
        },
        "Email": {
          "channelName": "Email",
          "isAvailable": true,
          "isEnabled": true,
          "isDefaultEnabled": true
        },
        "Push": {
          "channelName": "Push",
          "isAvailable": false,
          "isEnabled": false,
          "isDefaultEnabled": false
        }
      }
    }
    // ... other notification types
  ]
}
```

**Response Fields:**

- `profileId`: The UUID of the user's profile
- `updatedAt`: Timestamp of when preferences were last updated (null if no custom preferences exist)
- `doNotDisturbUntil`: Optional timestamp indicating do-not-disturb mode expiration
- `notificationTypes`: Array of notification type preference objects

**NotificationTypePreference Fields:**

- `typeKey`: Unique identifier for the notification type (e.g., "GroupMemberAdded")
- `displayName`: Human-readable name of the notification type
- `description`: Description of when this notification is triggered
- `channelPreferences`: Dictionary mapping channel names to channel preference objects

**ChannelPreference Fields:**

- `channelName`: Name of the channel (InApp, Email, Push)
- `isAvailable`: Whether this channel can be used for this notification type (determined by NotificationType configuration)
- `isEnabled`: Whether the user has enabled this channel for this notification type
- `isDefaultEnabled`: The default setting for this channel when no user preference exists

### PUT /api/notifications/preferences/detailed

Updates the user's notification preferences.

**Authentication:** Required (JWT Bearer token)

**Request Body:**

```json
{
  "notificationTypes": [
    {
      "typeKey": "GroupMemberAdded",
      "channelPreferences": {
        "InApp": true,
        "Email": false,
        "Push": true
      }
    },
    {
      "typeKey": "GroupAccessRequested",
      "channelPreferences": {
        "InApp": true,
        "Email": true,
        "Push": false
      }
    }
    // ... other notification types
  ]
}
```

**Response: 204 No Content**

**Request Validation:**

- Only channels marked as `isAvailable` for a notification type can be enabled
- Attempting to enable an unavailable channel will be ignored (the preference will be set to false)
- All notification types and channels must be included in the request

## Notification Types

The system supports the following notification types with their available channels:

| Type Key | Display Name | Available Channels |
|----------|--------------|-------------------|
| GroupMemberAdded | Group Member Added | InApp, Email, Push |
| GroupMemberRemoved | Group Member Removed | InApp, Email, Push |
| GroupAccessRequested | Group Access Requested | InApp, Email |
| ConferenceCreated | Conference Created | InApp, Email, Push |
| ConferenceUpdated | Conference Updated | InApp, Email, Push |
| ProfileFollowedConference | Profile Followed Conference | InApp, Email, Push |
| PresentationUpdated | Presentation Updated | InApp, Email, Push |
| PresentationScheduleChanged | Presentation Schedule Changed | InApp, Email, Push |
| ProfileCreated | Profile Created | InApp, Email, Push |
| ProfileUpdated | Profile Updated | InApp, Email, Push |
| ProfileCheckedIn | Profile Checked In | InApp, Email, Push |
| ProfileConferenceAttendanceChanged | Conference Attendance Changed | InApp, Email, Push |

## Data Flow

1. **GET Request Flow:**
   - Extract profile ID from JWT claims
   - Fetch user's saved preferences from Table Storage (if they exist)
   - Fetch all notification types from the NotificationTypeService
   - Merge the data:
     - For each notification type, iterate through each channel
     - If user has no preference, use the type's default setting
     - Mark channels as available based on the type's AvailableChannels configuration
     - Return combined data structure

2. **PUT Request Flow:**
   - Extract profile ID from JWT claims
   - Validate that only available channels are being enabled
   - Build TypeChannelPreferences dictionary from request
   - Create/Update NotificationPreferences domain model
   - Persist to Table Storage via Upsert operation

## Integration with Other Features

### Do Not Disturb Mode

The API also supports a separate do-not-disturb endpoint:
- `POST /api/notifications/preferences/do-not-disturb` - Set do-not-disturb until a specific time

When a user is in do-not-disturb mode, all non-critical notifications are held until the specified time.

### Preference Validation

- Channels that are not available for a notification type cannot be manually enabled
- The system enforces this constraint by setting unavailable channels to false regardless of the requested value
- Clients should respect the `isAvailable` flag when building UI controls

## Example Usage

### JavaScript/TypeScript

```typescript
// Fetch current preferences
const response = await fetch('/api/notifications/preferences/detailed', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const preferences = await response.json();

// Update preferences
await fetch('/api/notifications/preferences/detailed', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    notificationTypes: preferences.notificationTypes.map(type => ({
      typeKey: type.typeKey,
      channelPreferences: Object.fromEntries(
        Object.entries(type.channelPreferences)
          .filter(([_, pref]) => pref.isAvailable)
          .map(([name, pref]) => [name, pref.isEnabled])
      )
    }))
  })
});
```

### cURL

```bash
# Get preferences
curl -H "Authorization: Bearer <token>" \
  https://api.attendr.live/api/notifications/preferences/detailed

# Update preferences
curl -X PUT \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d @preferences.json \
  https://api.attendr.live/api/notifications/preferences/detailed
```

## Error Handling

| Status Code | Description |
|------------|-------------|
| 200 | Preferences retrieved successfully |
| 204 | Preferences updated successfully |
| 401 | Unauthorized - invalid or missing JWT token |
| 500 | Server error - see response body for details |

## Performance Considerations

- Preferences are cached in the NotificationTypeService singleton
- User preferences are fetched on-demand from Table Storage
- Combining data is done in-memory and is fast for the limited number of notification types
- Consider client-side caching of preferences if making frequent requests
