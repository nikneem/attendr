# Notification Preferences Frontend Implementation

## Overview
Created a comprehensive Angular frontend for managing notification preferences at `/app/preferences/notifications`. The page displays all available notification types with their descriptions and allows users to toggle notification channels with real-time server synchronization.

## Created Files

### Components
- **[notification-preferences-page.component.ts](src/App/src/app/pages/private/preferences/notification-preferences-page.component.ts)**
  - Main component handling preference loading and updates
  - Uses Angular signals for reactive state management
  - Integrates with NotificationPreferencesService for API calls
  - Handles channel toggle logic with availability constraints

- **[notification-preferences-page.component.html](src/App/src/app/pages/private/preferences/notification-preferences-page.component.html)**
  - Displays list of notification types with names and descriptions
  - Shows toggle switches for each available channel
  - Disables toggles for unavailable channels
  - Responsive design with loading and empty states

- **[notification-preferences-page.component.scss](src/App/src/app/pages/private/preferences/notification-preferences-page.component.scss)**
  - Custom styling for preference cards and toggle switches
  - Responsive layout (desktop and mobile)
  - Theme-aware colors using PrimeNG CSS variables
  - Smooth transitions and hover effects

### Services
- **[notification-preferences.service.ts](src/App/src/app/shared/services/notification-preferences.service.ts)**
  - Angular HttpClient service for API communication
  - Methods for fetching and updating detailed preferences
  - Interfaces with backend endpoints:
    - `GET /api/notifications/preferences/detailed`
    - `PUT /api/notifications/preferences/detailed`

### Models/DTOs
- **[notification-preferences-detail-dto.ts](src/App/src/app/shared/models/notification-preferences-detail-dto.ts)**
  - `NotificationPreferencesDetailDto`: Top-level response with profileId, updatedAt, doNotDisturbUntil
  - `NotificationTypePreferenceDto`: Per-type configuration with typeKey, displayName, description, and channel preferences
  - `ChannelPreferenceDto`: Per-channel setting with channelName, isAvailable, isEnabled, isDefaultEnabled

- **[update-notification-preferences-request.ts](src/App/src/app/shared/models/update-notification-preferences-request.ts)**
  - `UpdateDetailedPreferencesRequest`: Request structure for updating preferences
  - `UpdateNotificationTypePreferenceRequest`: Per-type preference update with channel toggles

### Routing
- **[app.routes.ts](src/App/src/app/app.routes.ts)** - Updated
  - Added new route: `{ path: 'preferences/notifications', component: NotificationPreferencesPageComponent }`
  - Route is protected by `AutoLoginAllRoutesGuard` requiring JWT authentication

## Features

### Display Features
✅ **Notification Type Listing**
- Shows all available notification types with displayName and description
- Clean card-based layout

✅ **Channel Management**
- Toggle switches for each channel (InApp, Email, Push)
- Visual indicators showing "On" or "Off" status
- Channels marked as unavailable are disabled and visually distinguished

✅ **Real-Time Sync**
- Changes to channel preferences are immediately saved to the server
- Toast notifications inform user of success/failure
- Loading state feedback with spinner

✅ **Responsive Design**
- Desktop layout: channel toggles displayed to the right of each type
- Mobile layout: toggles stack vertically for better touch interaction
- Optimized for various screen sizes

### Functional Features
✅ **Smart Constraints**
- Only available channels can be toggled
- Unavailable channels are disabled and cannot be changed
- Respects server-side channel availability configuration

✅ **Default Handling**
- Uses user's saved preferences if available
- Falls back to default settings from NotificationType configuration
- Shows when preferences were last updated

✅ **Error Handling**
- Graceful error messages for failed API calls
- Retry capability by reloading the page
- Console logging for debugging

✅ **Performance**
- Efficient data handling with signal-based reactivity
- TrackBy functions for optimized list rendering
- Lazy loading of notification types and preferences

## User Flow

1. **Load**: User navigates to `/app/preferences/notifications`
2. **Authentication**: AutoLoginAllRoutesGuard ensures user is authenticated
3. **Fetch**: Component loads user's notification preferences and types via API
4. **Display**: Preferences rendered with current settings and channel availability
5. **Interact**: User toggles channel switches to enable/disable notifications
6. **Save**: Changes immediately sent to backend via PUT request
7. **Feedback**: Toast notification shows success/failure
8. **Update**: UI updates to reflect saved state

## API Integration

### Request/Response Flow
```
GET /api/notifications/preferences/detailed
├─ Requires JWT authentication
├─ No request body
└─ Returns NotificationPreferencesDetailDto

PUT /api/notifications/preferences/detailed
├─ Requires JWT authentication
├─ Body: UpdateDetailedPreferencesRequest
└─ Returns 204 No Content
```

### Backend Endpoints
These endpoints were created as part of the backend implementation:
- [NotificationPreferencesDetailEndpoints.cs](../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/NotificationPreferencesDetailEndpoints.cs)

## Notification Types Managed

The page manages preferences for 12 notification types:
- GroupMemberAdded (InApp, Email, Push)
- GroupMemberRemoved (InApp, Email, Push)
- GroupAccessRequested (InApp, Email)
- ConferenceCreated (InApp, Email, Push)
- ConferenceUpdated (InApp, Email, Push)
- ProfileFollowedConference (InApp, Email, Push)
- PresentationUpdated (InApp, Email, Push)
- PresentationScheduleChanged (InApp, Email, Push)
- ProfileCreated (InApp, Email, Push)
- ProfileUpdated (InApp, Email, Push)
- ProfileCheckedIn (InApp, Email, Push)
- ProfileConferenceAttendanceChanged (InApp, Email, Push)

## Technical Details

### Angular Features Used
- **Signals**: For reactive state management (`signal<>()`)
- **RxJS**: For async operations with `subscribe()`
- **Common Module**: ngIf, ngFor, CommonModule utilities
- **FormsModule**: Input bindings and form handling
- **PrimeNG**: UI components (Button, Card, ProgressSpinner, Toast)

### CSS Features
- **CSS Variables**: PrimeNG theme integration
- **CSS Grid/Flexbox**: Responsive layouts
- **Custom Toggles**: Styled checkbox with sliding animation
- **Media Queries**: Mobile-friendly design

### Component Structure
```typescript
NotificationPreferencesPageComponent
├── State (Signals)
│   ├── preferences: NotificationPreferencesDetailDto | null
│   ├── isLoading: boolean
│   └── isSaving: boolean
├── Services (Injected)
│   ├── NotificationPreferencesService
│   └── MessageService
├── Methods
│   ├── loadPreferences(): void
│   ├── onChannelToggle(): void
│   ├── savePreferences(): void
│   └── Helper methods
└── Template
    ├── Header
    ├── Loading State
    ├── Preferences List
    └── Empty State
```

## Build Status
✅ Angular Frontend: Builds successfully  
✅ .NET Backend: Compiles without errors related to this feature  
✅ Routes: Properly configured in app.routes.ts

## Future Enhancements

Potential improvements for future iterations:
- Batch save instead of saving on each toggle
- Undo/Reset to defaults functionality
- Notification preview/test feature
- Do-not-disturb scheduling UI integration
- Notification history view
- Advanced filtering and sorting options
