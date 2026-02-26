# Spec: Contract Tests — Notification Preferences Detailed Payload

## Summary
Add tests that enforce the JSON contract between backend `GET /api/notifications/preferences/detailed` and the Angular DTO expectations so changes don’t silently break the UI.

## Context
- Angular DTOs live under ../../src/App/src/app/shared/models.
- Backend detailed preferences endpoint lives in ../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/NotificationPreferencesDetailEndpoints.cs.

## Goals
- Detect breaking contract changes in CI.
- Provide a safe path for adding new types/channels.

## Approach
Repo-native approach:
- Add backend integration test that calls the endpoint and validates required fields and invariants:
  - top-level `profileId`, `notificationTypes[]`
  - each type has `typeKey`, `displayName`, `channelPreferences`
  - each channel has `isAvailable`, `isEnabled`, `isDefaultEnabled`

Optional follow-up: add OpenAPI-based TS generation (out of scope for this spec).

## Acceptance criteria
- Test fails if backend removes/renames key fields.

## Test plan
- Add tests to Notifications.Tests using WebApplicationFactory.
