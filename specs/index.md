# Specs Index

## Frontend
- [Multi-language support (i18n)](frontend/multi-language-support.md) ✅

## Conferences
- [Conference ownership (creator, visibility override, edit access)](conferences/conference-ownership.md)
- [Manual conference content management (speakers, rooms, presentations)](conferences/manual-conference-content-management.md)

## Notifications UX
- [Notification preferences: batch save + undo](notifications/notification-preferences-batch-save.md)
- [Push onboarding: install → permission → subscribe → test](notifications/push-onboarding-flow.md) ✅
- [Do Not Disturb scheduling UI](notifications/do-not-disturb-ui.md)
- [Notification history: “why did I get this?”](notifications/notification-history-why.md)

## Backend hardening
- [Date & time conventions](backend/date-time-conventions.md)
- [Notification preferences: optimistic concurrency](backend/preferences-optimistic-concurrency.md)
- [Notification preferences: forward-compatible updates](backend/preferences-forward-compatible-updates.md)
- [Standardize errors with ProblemDetails](backend/problem-details-error-contract.md)

## Observability & quality
- [Metrics for preferences + push flows](observability/notifications-metrics.md)
- [Readiness checks for dependencies](observability/readiness-health-checks.md)
- [Contract tests: preferences detailed payload](quality/contract-tests-notification-preferences.md)
- [Domain invariants test suite: channels + DND](quality/domain-invariants-notifications.md)
