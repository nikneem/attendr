# Spec: Notification History — “Why did I get this?”

## Summary
Improve the existing notifications feed by exposing explanation details (“why”) and displaying delivery status/context for a notification. This helps users trust the system and validate preference changes.

## Context
- Backend endpoints exist in ../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/NotificationsEndpoints.cs.
- Frontend service exists in ../../src/App/src/app/shared/services/notifications.service.ts.
- Notifications include `TypeKey`, `EntityRefs`, `Url`, and computed per-channel delivery info at creation time (see ../../src/Notifications/HexMaster.Attendr.Notifications/Services/NotificationService.cs).

## Goals
- Let users see, per notification:
  - type (human readable) when possible
  - delivery status per channel (enabled + status)
  - whether DND likely caused skipping
  - key entity references (when present)

## Non-goals
- No new page.
- No complex filtering.

## UX / behavior
- Update the notifications popover items to support “expand details” inline.
- On expand, the UI loads details via existing `GET /api/notifications/{id}`.
- For a human-readable type name:
  - Fetch `GET /api/notifications/types` once (cached in a frontend service), then map `typeKey → displayName`.
  - If types cannot be loaded, fall back to showing `typeKey`.
- Details render:
  - Type: `<DisplayName>` (or `typeKey` fallback)
  - Reason (computed client-side from `channelDeliveries`):
    - If a channel has `enabled=true` but `status=Skipped`, show “Delivery skipped (likely Do Not Disturb or server-side suppression).”
    - If `enabled=false` and `status=Skipped`, show “Delivery skipped because this channel is turned off.”
    - If `status` indicates delivery/pending, show “Delivery scheduled/sent because this channel is enabled.”
  - Delivery summary per channel (Enabled + Status [+ ErrorMessage when present])
  - Links: use `url` if present; list `entityRefs` as key/value when present

## API / contracts
- Prefer keeping list payload small and using `GET /{id}` for details (already exists).
- Use `GET /api/notifications/types` to resolve `displayName` for `typeKey`.
- If the details DTO does not include `channelDeliveries` / `entityRefs`, extend mapping (currently they exist in the DTO contract).

## Observability
- Counter: details endpoint calls.

## Acceptance criteria
- User can expand an item and understand delivery decisions.
- DND skipping is visible.

## Test plan
- Backend mapping tests for the details DTO.
- Frontend component tests for expand behavior.
