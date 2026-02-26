# Spec: Do Not Disturb (DND) — Scheduling UI

## Summary
Expose the existing Do Not Disturb capability as a simple scheduling UI on the notification preferences page, allowing users to mute all notifications until a selected time.

## Context
- Backend endpoint exists: `POST /api/notifications/preferences/do-not-disturb` in ../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/NotificationPreferencesEndpoints.cs.
- Notification creation already marks deliveries as skipped during DND in ../../src/Notifications/HexMaster.Attendr.Notifications/Services/NotificationService.cs.

## Goals
- Let user set DND until a timestamp (quick presets + custom).
- Let user clear DND immediately.
- Clearly show current DND status and expiration.

## Non-goals
- No per-channel DND.
- No recurring schedules.

## UX / behavior
- Add a “Do Not Disturb” card above the notification type list.
- Display:
  - Current status: Off / On until <time>
  - Quick actions: 1 hour, 4 hours, Until tomorrow, Custom time
  - “Turn off” button when active
- When user selects a time:
  - call `POST /api/notifications/preferences/do-not-disturb` with `{ doNotDisturbUntil: <utc> }`
  - refresh preferences (re-fetch detailed preferences)

### Time handling
- UI selection uses local time; send UTC to backend.
- If a past time is selected, treat as “Turn off”.

### Semantics
- DND affects all channels uniformly.
- This spec does not change delivery semantics; it only exposes controls.

## Observability
- Counter metric: DND set/cleared (see metrics spec).

## Acceptance criteria
- User can set DND and see it reflected on reload.
- User can clear DND.
- Expiration time is displayed in user locale.

## Test plan
- Frontend: time conversion + button flows.
- Backend: endpoint integration test verifying repository update.
