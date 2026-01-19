# Notifications Button Component Implementation

## Overview
Added a notifications button component to the private page template that displays a bell icon with a badge showing the number of unread notifications. Clicking the button opens a popover with the list of notifications.

## Created Files

### Models
- **[notification-dto.ts](src/App/src/app/shared/models/notification-dto.ts)**
  - TypeScript interface for notification data
  - Includes fields: id, title, message, url, isRead, createdAt, etc.
  - Matches the backend NotificationDto structure

### Services
- **[notifications.service.ts](src/App/src/app/shared/services/notifications.service.ts)**
  - Angular service for notification operations
  - Methods:
    - `getNotifications(includeRead)` - Fetch notifications
    - `getUnreadCount()` - Get count of unread notifications
    - `markAsRead(id)` - Mark a notification as read
    - `markAllAsRead()` - Mark all notifications as read
    - `deleteNotification(id)` - Delete a notification
    - `pollNotifications(intervalMs)` - Observable that polls for notifications
    - `pollUnreadCount(intervalMs)` - Observable that polls for unread count
  - Default polling interval: 180000ms (3 minutes)
  - Uses RxJS signals for reactive unread count

### Components
- **[notifications-button.component.ts](src/App/src/app/shared/components/notifications-button/notifications-button.component.ts)**
  - Standalone Angular component
  - Features:
    - Bell icon with badge showing unread count
    - Popover showing list of notifications
    - Automatic polling every 3 minutes for unread count
    - Mark as read/delete actions per notification
    - Mark all as read button
    - Time formatting (e.g., "5m ago", "2h ago")
    - Click notification to navigate to URL
  - Uses PrimeNG components: Popover, Badge, Button, ProgressSpinner, Tooltip

- **[notifications-button.component.html](src/App/src/app/shared/components/notifications-button/notifications-button.component.html)**
  - Template showing bell button and popover
  - Displays loading state, empty state, and notification list
  - Each notification shows title, message, timestamp
  - Action buttons for mark as read and delete

- **[notifications-button.component.scss](src/App/src/app/shared/components/notifications-button/notifications-button.component.scss)**
  - Styling for button, badge, popover, and notification list
  - Responsive design with max-width for mobile
  - Visual distinction for unread notifications
  - Hover effects and transitions

## Modified Files

### Templates
- **[private-page-template.html](src/App/src/app/templates/private/private-page-template/private-page-template.html)**
  - Added `<attn-notifications-button />` next to account menu
  - Wrapped in new `.header-actions` container

- **[private-page-template.ts](src/App/src/app/templates/private/private-page-template/private-page-template.ts)**
  - Added import for `NotificationsButtonComponent`
  - Added to component imports array

- **[private-page-template.scss](src/App/src/app/templates/private/private-page-template/private-page-template.scss)**
  - Added `.header-actions` styles with flexbox layout
  - Gap between notifications button and account menu

## API Endpoints Used

The service connects to the following backend endpoints:

- `GET /api/notifications?includeRead={boolean}` - Get notifications
- `GET /api/notifications/unread/count` - Get unread count
- `POST /api/notifications/{id}/read` - Mark as read
- `POST /api/notifications/read-all` - Mark all as read
- `DELETE /api/notifications/{id}` - Delete notification

## Features

1. **Real-time Badge Updates**
   - Polls unread count every 3 minutes
   - Updates badge automatically
   - Signal-based reactive state

2. **Popover with Notification List**
   - Shows only unread notifications by default
   - Click to open/close popover
   - Loading state with spinner
   - Empty state message

3. **Notification Actions**
   - Click notification to navigate to URL (if provided)
   - Mark individual notification as read
   - Mark all notifications as read
   - Delete individual notification

4. **Time Formatting**
   - "Just now" for < 1 minute
   - "Xm ago" for minutes
   - "Xh ago" for hours
   - "Xd ago" for days
   - Date format for older notifications

5. **Visual Indicators**
   - Red badge with count on bell icon
   - Blue dot on unread notifications
   - Different background for unread items
   - Hover effects

## Usage

The notifications button is automatically displayed in the private page template (used for all authenticated pages). No additional configuration is needed.

The component handles its own state and polling, starting automatically when initialized and cleaning up when destroyed.

## Configuration

To change the polling interval, modify the `pollUnreadCount()` call in `notifications-button.component.ts`:

```typescript
// Default is 180000ms (3 minutes)
this.pollingSubscription = this.notificationsService.pollUnreadCount(300000) // 5 minutes
```
