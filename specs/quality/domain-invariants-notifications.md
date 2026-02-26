# Spec: Domain Invariants Tests — Notification Channels + DND

## Summary
Add a focused unit test suite to ensure notification business rules remain correct as features evolve (channels availability/defaults, DND behavior, stacking).

## Context
- Notification delivery status is computed in ../../src/Notifications/HexMaster.Attendr.Notifications/Services/NotificationService.cs.

## Goals
- Prevent regressions in:
  - defaults vs user preferences
  - DND causes deliveries to be skipped
  - stacking behavior increments count

## Test cases
- No saved preferences → type defaults used.
- Preference disables Email → Email delivery is skipped.
- Active DND → all channels skipped.
- Stacking allowed + same stackKey → count increments and last occurred updates.

## Acceptance criteria
- Tests run fast and cover the core invariants.

## Test plan
- xUnit + Moq + Bogus.
