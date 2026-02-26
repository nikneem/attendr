# Spec: Readiness Health Checks for Notifications Dependencies

## Summary
Add readiness checks that validate key dependencies (Table Storage connectivity and required VAPID configuration) so deployments fail fast and Aspire shows clear health states.

## Context
- Service defaults add a self liveness check only, in ../../src/Aspire/HexMaster.Attendr.Aspire/HexMaster.Attendr.Aspire.ServiceDefaults/Extensions.cs.
- Notifications depends on Table Storage and VAPID config.

## Goals
- `/alive` remains a cheap liveness probe.
- `/health` becomes a readiness probe including dependencies.

## Proposed checks
- `table-storage` (tag: `ready`): verify TableServiceClient can access required tables.
- `vapid-config` (tag: `ready`): verify `VAPID:PublicKey` and `VAPID:PrivateKey` are configured.

## Acceptance criteria
- If Table Storage is down, `/health` is unhealthy while `/alive` is healthy.

## Test plan
- Integration tests with missing config.
