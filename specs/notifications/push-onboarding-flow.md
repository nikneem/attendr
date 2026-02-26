# Spec: Push Notifications — Deterministic Onboarding Flow

## Status
**Implemented** — 2026-02-26  
Frontend-only. Push setup card with four deterministic steps (Install → Permission → Register → Test), auto-registration via `effect()` on SW subscription signal, and inline per-preference push-not-configured warning.

## Summary
Make push notifications setup understandable and reliable by presenting a single guided flow: install PWA (when required) → grant permission → register subscription → send test. The page clearly shows current status and allows unsubscribe.

## Context
- Frontend already includes PWA detection, install prompt handling, and permission banners in ../../src/App/src/app/pages/private/preferences/notification-preferences-page.component.ts.
- Backend subscription endpoints exist in ../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/PushSubscriptionsEndpoints.cs:
  - `POST /api/notifications/subscriptions`
  - `DELETE /api/notifications/subscriptions`
  - `GET /api/notifications/subscriptions/test`
- Frontend API wrapper exists in ../../src/App/src/app/shared/services/notification-subscriptions.service.ts.

## Goals
- Provide a predictable “setup checklist” UI with explicit status per step.
- Make it obvious why push can’t be enabled (not installed, permission denied, missing subscription).
- Ensure backend knows about the current browser subscription before the user relies on Push.

## Non-goals
- No new notification channels.
- No advanced device management UI.

## Users & scenarios
- Mobile user wants push; they are not installed as PWA.
- User previously denied permission; needs instructions to re-enable.
- User switched browsers/devices; needs to re-register subscription.

## UX / behavior
Add a “Push setup” card above the preference list.

### Step model
Each step has `Complete | Incomplete | Blocked`:
1. **Install** (required only when mobile + install prompt available + not already standalone).
2. **Permission** (`Notification.permission === 'granted'` is Complete; `denied` is Blocked).
3. **Register this device** (Complete when backend has a record for the current subscription endpoint).
4. **Send test** (action; displays result `sentCount`).

### Registration strategy (no new backend endpoint required)
- When permission is granted and a subscription exists, perform an idempotent `POST /api/notifications/subscriptions`.
- Treat a 204 response as “registered”.
- If it fails, show a non-blocking error + retry.

### Unsubscribe
- Button “Unsubscribe this device”:
  - calls `DELETE /api/notifications/subscriptions` with `{ endpoint }`.
  - then calls browser `subscription.unsubscribe()`.

### Relationship to preferences
- If Push is enabled for a type but the device is not registered:
  - show inline warning “Push enabled but not configured on this device”.
  - do not block Save; keep UX simple.

## Observability
- See metrics spec for counters around permission outcomes, registration attempts, unsubscribe, and test sends.

## Acceptance criteria
- First-time user can complete setup without guessing what to do next.
- Permission denied state gives actionable explanation (cannot re-prompt, must change browser settings).
- Test send result is visible to the user.

## Test plan
- Frontend unit tests for step state computation.
- Backend tests for input validation (existing) and test endpoint response shape.

## Rollout
- UI-first; no data migration.

## Open questions
- Should test sends be rate limited per profile/device? Default: rely on monitoring first.
