# Spec: Notification Preferences — Optimistic Concurrency (ETag)

## Summary
Prevent silent overwrites when multiple sessions/devices update notification preferences by introducing optimistic concurrency using Azure Table Storage ETags.

## Context
- Preferences are stored in Table Storage entity ../../src/Notifications/HexMaster.Attendr.Notifications.Data.TableStorage/Entities/NotificationPreferencesEntity.cs (contains `ETag`).
- Current repository upserts without ETag checks in ../../src/Notifications/HexMaster.Attendr.Notifications.Data.TableStorage/Repositories/TableStorageNotificationPreferencesRepository.cs.

## Goals
- Detect conflicting updates and return 409.
- Enable frontend to refresh and retry.

## Non-goals
- No multi-client merge UI.

## API / contracts
- Extend detailed preferences response with `version` (string), set to entity ETag.
- Extend update request with `version`.
- On conflict: return 409 `ProblemDetails` (see error contract spec).

## Data/repository behavior
- Read returns ETag.
- Update uses conditional update with `If-Match`.
- If entity does not exist:
  - accept `version=null` to create
  - reject non-null versions with 409

## Acceptance criteria
- Two clients saving concurrently: second receives 409 and does not overwrite.

## Test plan
- Repository tests using Azurite for ETag behavior.
- API integration tests for 409 response.
