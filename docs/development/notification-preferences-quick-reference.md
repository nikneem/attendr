# Notification Preferences Feature - Developer Quick Reference

## Route
```
/app/preferences/notifications
```

## Key Files

### Frontend
| File | Purpose |
|------|---------|
| [notification-preferences-page.component.ts](../../src/App/src/app/pages/private/preferences/notification-preferences-page.component.ts) | Main component logic |
| [notification-preferences-page.component.html](../../src/App/src/app/pages/private/preferences/notification-preferences-page.component.html) | UI template |
| [notification-preferences-page.component.scss](../../src/App/src/app/pages/private/preferences/notification-preferences-page.component.scss) | Styling |
| [notification-preferences.service.ts](../../src/App/src/app/shared/services/notification-preferences.service.ts) | API service |
| [notification-preferences-detail-dto.ts](../../src/App/src/app/shared/models/notification-preferences-detail-dto.ts) | Data models (response) |
| [update-notification-preferences-request.ts](../../src/App/src/app/shared/models/update-notification-preferences-request.ts) | Data models (request) |

### Backend
| File | Purpose |
|------|---------|
| [NotificationPreferencesDetailEndpoints.cs](../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/NotificationPreferencesDetailEndpoints.cs) | API endpoints |
| [NotificationPreferencesDetailDto.cs](../../src/Notifications/HexMaster.Attendr.Notifications.Abstractions/DTOs/NotificationPreferencesDetailDto.cs) | Response DTOs |

## API Endpoints

### GET /api/notifications/preferences/detailed
Fetch user's notification preferences combined with notification type configuration.

**Response:**
```json
{
  "profileId": "uuid",
  "updatedAt": "2024-01-16T10:30:00Z",
  "doNotDisturbUntil": null,
  "notificationTypes": [
    {
      "typeKey": "GroupMemberAdded",
      "displayName": "Group Member Added",
      "description": "Notification when a member is added to a group",
      "channelPreferences": {
        "InApp": { "channelName": "InApp", "isAvailable": true, "isEnabled": true, "isDefaultEnabled": true },
        "Email": { "channelName": "Email", "isAvailable": true, "isEnabled": false, "isDefaultEnabled": false },
        "Push": { "channelName": "Push", "isAvailable": true, "isEnabled": true, "isDefaultEnabled": true }
      }
    }
  ]
}
```

### PUT /api/notifications/preferences/detailed
Update user's notification preferences.

**Request:**
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
    }
  ]
}
```

## Component Usage

### Import in routing
```typescript
import { NotificationPreferencesPageComponent } from './pages/private/preferences/notification-preferences-page.component';

{ path: 'preferences/notifications', component: NotificationPreferencesPageComponent }
```

### Inject service
```typescript
private readonly preferencesService = inject(NotificationPreferencesService);
```

## Data Flow

```
User Navigation
↓
Route Guard (AutoLoginAllRoutesGuard)
↓
Component Load
↓
loadPreferences() → GET /api/notifications/preferences/detailed
↓
Render Preferences
↓
User Toggles Channel
↓
onChannelToggle()
↓
savePreferences() → PUT /api/notifications/preferences/detailed
↓
Toast Notification
```

## Styling Classes

| Class | Purpose |
|-------|---------|
| `.notification-preferences-container` | Main container |
| `.preferences-header` | Top header with title |
| `.notification-type-card` | Individual notification type card |
| `.type-info` | Type name and description |
| `.channels-container` | Container for channel toggles |
| `.channel-toggle` | Single channel toggle |
| `.toggle-switch` | Native checkbox styled as toggle |
| `.toggle-label` | "On" / "Off" text |

## Environment Configuration

Ensure the API URL is configured in your environment file:

```typescript
// environments/environment.ts
export const environment = {
  apiUrl: 'https://api.attendr.com',
  // ... other config
};
```

## Testing Checklist

- [ ] Page loads and displays all notification types
- [ ] Channel toggles are interactive
- [ ] Unavailable channels are disabled
- [ ] Toggle changes are saved immediately
- [ ] Success message appears after save
- [ ] Error message appears on API failure
- [ ] Page works on mobile devices
- [ ] Loading spinner appears while fetching
- [ ] Empty state displays if no types available
- [ ] User can navigate back to other pages

## Common Tasks

### Add new notification type
1. Update [NotificationTypeService.cs](../../src/Notifications/HexMaster.Attendr.Notifications/Services/NotificationTypeService.cs) to include new type with AvailableChannels
2. Type automatically appears in preferences page after deployment

### Disable channel for a type
1. Update AvailableChannels in NotificationType configuration
2. Channel toggle automatically disables in UI

### Modify UI styling
1. Edit [notification-preferences-page.component.scss](../../src/App/src/app/pages/private/preferences/notification-preferences-page.component.scss)
2. Uses PrimeNG CSS variables for theming compatibility

### Change API behavior
1. Modify endpoint in [NotificationPreferencesDetailEndpoints.cs](../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/NotificationPreferencesDetailEndpoints.cs)
2. Update corresponding service in frontend if needed
