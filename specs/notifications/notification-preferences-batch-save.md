# Spec: Notification Preferences — Batch Save + Undo

## Summary
Change the notification preferences page from “save on every toggle” to an explicit batch editing flow with Save/Cancel and a “Reset to defaults” action. This reduces accidental changes and improves reliability on slow/flaky networks.

## Context
- Current UI performs immediate writes per toggle in ../../src/App/src/app/pages/private/preferences/notification-preferences-page.component.ts.
- Current API uses `GET/PUT /api/notifications/preferences/detailed` in ../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/NotificationPreferencesDetailEndpoints.cs.
- Pain points:
  - Multiple rapid toggles trigger multiple writes; failures are hard to reconcile.
  - No quick way to undo or revert to defaults.

## Goals
- Allow users to change multiple toggles then commit once.
- Provide Cancel to revert unsaved changes.
- Provide “Reset to defaults” (client-side) and then Save.
- Preserve server-side constraints (unavailable channels cannot be enabled).

## Non-goals
- No filtering/sorting of notification types.
- No additional pages/modals; changes stay within the existing page.

## Users & scenarios
- User wants to disable all Email channels quickly, then Save.
- User toggles wrong channel and hits Cancel.
- User wants to “start over” with default channel settings.

## UX / behavior
- Page loads current preferences via `GET /api/notifications/preferences/detailed`.
- Local editable state is initialized from server response.
- Toggling a switch updates only local state and marks the page “dirty”.
- Page header area shows:
  - Save button (disabled when not dirty or when saving)
  - Cancel button (enabled when dirty)
  - Reset to defaults button (enabled when loaded; marks dirty)
- Saving:
  - Sends a single `PUT /api/notifications/preferences/detailed` with current local state.
  - On success: dirty flag cleared; show toast “Preferences saved”.
  - On failure: local state remains; show toast with retry.
- Cancel:
  - Restores local state to last successfully loaded/saved snapshot.

### Unavailable channels
- If `isAvailable=false`:
  - Toggle is disabled.
  - Local state should always hold `false` for that channel.

### Reset to defaults
- Reset uses the server-provided defaults:
  - For each type/channel where `isAvailable=true`, set `isEnabled = isDefaultEnabled`.
  - For `isAvailable=false`, set `isEnabled=false`.

### Accessibility
- Save/Cancel/Reset controls are keyboard reachable and have clear labels.
- Disabled toggles remain readable; do not rely solely on color.

## API / contracts
- Continue using existing detailed endpoints.
- Request shape remains `UpdateDetailedPreferencesRequest`.
- Concurrency/versioning is addressed in a separate spec.

## Domain & data
- No domain model changes required for batching.
- Client remains source of the updated map; server enforces availability.

## Observability
- UI: avoid noisy logs; rely on toasts.
- Backend: rely on existing HTTP instrumentation; additional metrics are covered in the metrics spec.

## Acceptance criteria
- User can toggle 10 switches and only 1 network write occurs when pressing Save.
- Cancel restores the pre-edit state.
- Reset to defaults matches `isDefaultEnabled` and does not enable unavailable channels.

## Test plan
- Frontend unit tests:
  - Dirty tracking toggles correctly.
  - Cancel/Reset behaviors.
  - Save only calls the update service once.
- Backend: no changes required.

## Rollout
- Ship as a UI-only change; no migration.

## Open questions
- Should we warn on navigation away when dirty? Default: no (keep minimal).
