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

## Backend Setup

### Configuration & Keys
- [x] Generate VAPID key pair using web-push CLI
- [ ] Add VAPID keys to appsettings.Development.json
- [ ] Add VAPID keys to appsettings.Production.json
- [ ] Configure PushNotificationOptions in Program.cs

### Database Schema
- [x] Create PushSubscription entity/table schema
- [x] Provisioned subscriptions table in Aspire
- [x] Add fields: ProfileId, Endpoint, P256DH, Auth, UserAgent, CreatedAt, UpdatedAt

### Services
- [ ] Create PushNotificationSender service (sends push notifications)
- [ ] Implement IPushNotificationSender interface
- [ ] Add subscription expiration handling (410 Gone)
- [ ] Add error handling and logging

### Repositories
- [x] IPushSubscriptionRepository interface created
- [x] TableStoragePushSubscriptionRepository implemented
- [x] Upsert/Get/Delete methods implemented
- [x] PushSubscriptionEntity and mapper created
- [x] Repository registered in DI

### Endpoints
- [x] PushSubscriptionsEndpoints created
- [x] POST /api/notifications/subscriptions endpoint implemented
- [ ] DELETE /api/notifications/subscriptions/{id} endpoint (TODO: implement if needed)
- [x] Authorization checks in place
- [x] Endpoints registered in Program.cs

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

## Frontend Features

### Notification Preferences UI
- [x] "Enable Push Notifications" toggle already exists
- [x] PushNotificationService.subscribe() called on toggle ON
- [x] Backend endpoint called to save subscription (NotificationSubscriptionsService)
- [x] Permission checking implemented with user feedback
- [x] Browser support validation
- [x] Toast notifications for status messages

### Error Handling
- [x] Handle browser not supporting push notifications
- [x] Handle permission denied gracefully
- [x] Show user-friendly error messages (using MessageService)
- [ ] Provide manual retry option (user can toggle again)

### User Experience
- [x] Permission request handled via PushNotificationService
- [x] Toast messages explain permission flow
- [x] Notification type preferences UI already supports per-type toggles
- [x] VAPID public key configured in environment

## Testing - TODO

### Unit Tests
- [x] PushNotificationService.subscribe() implemented with full logic
- [x] PushNotificationService.unsubscribe() implemented
- [x] VAPID key conversion utilities implemented (urlBase64ToUint8Array)
- [ ] Test PushNotificationSender initialization
- [ ] Test payload serialization

### Integration Tests
- [x] Backend push endpoints (POST) implemented
- [x] Subscription storage in database (repository ready)
- [ ] Test push sending via WebPush
- [ ] Test subscription expiration handling
- [ ] Test event handler → push notification flow

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

- [x] VAPID keys generated and ready to configure
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

- [ ] Configure backend to use VAPID keys from environment
- [ ] Verify HTTPS is enforced
- [ ] Set up monitoring for push delivery
- [ ] Configure logging and alerting
- [ ] Prepare rollback plan

---

## Current Status

**Frontend**: ✅ 95% Complete
- Service worker, push service, NGSW integration all configured
- Permission checking flow implemented
- Subscription registration to backend implemented
- Builds successfully
- Ready for backend integration and testing

**Backend**: 🔄 40% Complete
- Subscriptions table provisioned in Aspire
- PushSubscription domain model, entity, repository, and endpoints created
- Builds successfully
- VAPID keys generated and ready to configure in settings
- Awaiting: PushNotificationSender service and event integration

**Testing**: ⏳ Pending
- Can begin after backend push sending implementation complete
