# Spec: Metrics for Notification Preferences + Push Flows

## Summary
Add OTel metrics and a few business spans to make notification setup and preference changes observable in production.

## Context
- ADR 0008 requires OTel for metrics/traces.
- Aspire service defaults already add ASP.NET + HttpClient instrumentation in ../../src/Aspire/HexMaster.Attendr.Aspire/HexMaster.Attendr.Aspire.ServiceDefaults/Extensions.cs.

## Goals
- Measure adoption and failure rates for:
  - preference updates
  - DND set/clear
  - subscription register/unsubscribe
  - test notifications

## Metrics (backend)
Create a `NotificationsMetrics` meter:
- Counter `attendr.notifications.preferences.update` attributes: `result=success|failure|conflict`
- Counter `attendr.notifications.dnd.set`
- Counter `attendr.notifications.dnd.clear`
- Counter `attendr.notifications.push.register` attributes: `result=success|failure`
- Counter `attendr.notifications.push.unsubscribe` attributes: `result=success|failure`
- Counter `attendr.notifications.push.test.send` attributes: `sentCount`
- Histogram `attendr.notifications.push.send.duration_ms` attributes: `result=success|failure`

## Tracing
- ActivitySource `HexMaster.Attendr.Notifications` spans:
  - `Preferences.UpdateDetailed`
  - `PushSubscription.Register`
  - `PushNotification.TestSend`

## Logging
- Avoid logging full push endpoints; log host only.

## Acceptance criteria
- Metrics are emitted and visible through the configured exporter.

## Test plan
- Unit tests: metric wrapper methods callable.
- Manual: verify in Aspire dashboard/collector.
