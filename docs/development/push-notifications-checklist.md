# Push Notifications Integration Checklist

## PWA Infrastructure ✅ COMPLETE

- [x] Service worker created and compiled (`public/service-worker.js`)
- [x] Push event listener implemented
- [x] Notification click handler implemented  
- [x] Action button support added
- [x] Service worker registered in app.config.ts
- [x] Manifest.webmanifest configured with `display: "standalone"`
- [x] PushNotificationService created with TypeScript signals
- [x] VAPID key conversion utilities implemented
- [x] Frontend build successful

## Backend Setup - TODO

### Configuration & Keys
- [ ] Generate VAPID key pair using web-push CLI
- [ ] Add VAPID keys to appsettings.Development.json
- [ ] Add VAPID keys to appsettings.Production.json
- [ ] Configure PushNotificationOptions in Program.cs

### Database Schema
- [ ] Create PushSubscription entity/table schema
- [ ] Migration: Add PushSubscriptions table
- [ ] Add fields: Id, ProfileId, Endpoint, P256DH, Auth, UserAgent, CreatedAt, UpdatedAt

### NuGet Packages
- [ ] Install WebPush NuGet package
- [ ] Verify package compatibility with .NET 10

### Services
- [ ] Create PushNotificationSender service
- [ ] Implement IPushNotificationSender interface
- [ ] Add subscription expiration handling (410 Gone)
- [ ] Add error handling and logging

### Repositories
- [ ] Add SavePushSubscriptionAsync method to INotificationPreferencesRepository
- [ ] Add DeletePushSubscriptionAsync method
- [ ] Add GetPushSubscriptionsForUserAsync method
- [ ] Implement all methods in NotificationPreferencesRepository

### Endpoints
- [ ] Create PushSubscriptionsEndpoints
- [ ] POST /api/notifications/subscriptions (subscribe)
- [ ] DELETE /api/notifications/subscriptions/{id} (unsubscribe)
- [ ] Add authorization checks
- [ ] Register endpoints in Program.cs

### Event Integration
- [ ] Add push notifications to GroupAccessRequestedEvent handler
- [ ] Add push notifications to ConferenceUpdatedEvent handler
- [ ] Add push notifications to ConferenceCreatedEvent handler
- [ ] Add push notifications to PresentationScheduleChangedEvent handler
- [ ] Add push notifications to PresentationUpdatedEvent handler
- [ ] Implement retry logic for failed push sends

### Dapr Integration
- [ ] Configure Dapr pub/sub for notification events
- [ ] Wire event handlers to trigger push sends
- [ ] Test event-to-push flow end-to-end

## Frontend Features - TODO

### Notification Preferences UI
- [ ] Add "Enable Push Notifications" toggle to notification preferences page
- [ ] Call PushNotificationService.subscribe() on toggle ON
- [ ] Call backend endpoint to save subscription
- [ ] Display subscription status to user
- [ ] Show last updated timestamp

### Error Handling
- [ ] Handle browser not supporting push notifications
- [ ] Handle permission denied gracefully
- [ ] Show user-friendly error messages
- [ ] Provide manual retry option

### User Experience
- [ ] Show permission request before prompting for notification permission
- [ ] Display explanation of why app needs notifications
- [ ] Allow users to manage which notification types use push

## Testing - TODO

### Unit Tests
- [ ] Test PushNotificationService.subscribe()
- [ ] Test PushNotificationService.unsubscribe()
- [ ] Test VAPID key conversion utilities
- [ ] Test PushNotificationSender initialization
- [ ] Test payload serialization

### Integration Tests
- [ ] Test backend push endpoints (POST/DELETE)
- [ ] Test subscription storage in database
- [ ] Test push sending via WebPush
- [ ] Test subscription expiration handling
- [ ] Test event handler → push flow

### End-to-End Tests
- [ ] Subscribe to push on development server
- [ ] Trigger notification event
- [ ] Receive push notification on mobile device
- [ ] Click notification and verify navigation
- [ ] Unsubscribe and verify no more notifications

### DevTools Testing
- [ ] Test push event simulation in DevTools
- [ ] Test offline mode handling
- [ ] Verify service worker is active
- [ ] Check push event payload in DevTools logs

## Security & Performance - TODO

- [ ] Validate VAPID key format
- [ ] Implement rate limiting on push sends
- [ ] Add logging for all push operations
- [ ] Monitor subscription expiration rate
- [ ] Clean up expired subscriptions periodically
- [ ] Add monitoring/alerting for push failures
- [ ] Document security best practices for VAPID keys

## Documentation - TODO

- [ ] Update README.md with push notification feature
- [ ] Create admin guide for managing push notifications
- [ ] Document payload formats for each notification type
- [ ] Add troubleshooting guide
- [ ] Create developer setup guide

## Deployment - TODO

- [ ] Configure CI/CD to securely pass VAPID keys
- [ ] Verify HTTPS is enforced
- [ ] Set up monitoring for push delivery
- [ ] Configure logging and alerting
- [ ] Prepare rollback plan

---

## Current Status

**Frontend**: ✅ 100% Complete
- Service worker, push service, NGSW integration all configured
- Build passes without errors
- Ready for backend integration

**Backend**: 🔄 Not Started
- Documentation provided and ready
- Awaiting VAPID key generation and configuration

**Testing**: ⏳ Pending
- Can begin after backend setup complete

**Deployment**: ⏳ Pending
- Can begin after all testing complete
