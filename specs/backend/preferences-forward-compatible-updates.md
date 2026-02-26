# Spec: Notification Preferences — Forward-Compatible Updates

## Summary
Make preferences updates resilient to new notification types and channels by allowing partial updates and ignoring unknown fields, while preserving availability constraints.

## Context
- Detailed preferences API currently expects clients to send all types/channels (per docs).
- This can break older clients when new types are added.

## Goals
- Allow clients to update only changed types/channels.
- Ensure server stays authoritative about availability/defaults.

## Non-goals
- No UI filtering.

## API / contracts
Add a new endpoint:
- `PATCH /api/notifications/preferences/detailed`
  - Body: `{ version?, changes: [{ typeKey, channelPreferences: { InApp?, Email?, Push? } }] }`
  - Missing keys mean “no change”.
  - Unknown `typeKey`: ignore and return 200 with warnings in logs (do not fail whole request).
- Keep existing `PUT` for backward compatibility.

## Constraint enforcement
- For each change:
  - if a channel is unavailable, force `false`.
  - if a channel is omitted, keep existing.

## Acceptance criteria
- PATCH clients survive introduction of new types.
- PATCH update of a single type does not require sending full payload.

## Test plan
- API tests covering partial update and unknown type handling.
