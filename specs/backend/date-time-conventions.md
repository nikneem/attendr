# Spec: Date & time conventions

## Summary
Standardize how Attendr represents, stores, exchanges, and displays time.

- Backend uses `DateTimeOffset` for instants (timestamps and scheduled start/end times).
- Datastores persist instants in UTC.
- Imports from external systems treat any offset-less values as UTC.
- Frontend displays instants in the user’s browser locale and timezone.

## Context
The solution currently mixes `DateTime` and `DateTimeOffset` across domain models, DTOs, data entities, and integration events. This makes it easy to accidentally lose offset/zone context, misinterpret “unspecified” times, and display incorrect local times.

Related code already aligned:
- Core domain base classes and domain events use `DateTimeOffset` (`CreatedOn`/`ModifiedOn`, `OccurredAt`).

Notable misalignments exist in:
- Shared integration events (`IntegrationEvent.OccurredAt` uses `DateTime`).
- Notifications models and contracts (several `CreatedAt`/`ExpiresAt`/`DoNotDisturbUntil` values are `DateTime`).
- Presence/Groups scheduled start/end values (several `StartDateTime`/`EndDateTime` values are `DateTime`).

## Goals
- One clear, enforceable convention for all timestamps and scheduled times.
- Make cross-service contracts unambiguous and consistent.
- Ensure frontend displays times correctly without per-feature fixes.

## Non-goals
- Migrating all existing code and storage in this spec.
- Introducing a new “time” library or complex time-zone selection UX.

## Users & scenarios
- Attendees view conference and presentation schedules in their local timezone.
- Services exchange schedule and check-in events without ambiguity.

## Conventions

### 1) Terminology
- **Instant**: a point in time on the global timeline (e.g., “notification created at”, “check-in occurred at”).
- **Local wall-clock time**: a calendar time in a specific timezone (not currently modeled explicitly in Attendr).
- **Date-only**: a calendar date without time (e.g., “conference starts on 2026-05-01”).

### 2) Backend (.NET)

#### Instants and scheduled date-times
- Use `DateTimeOffset` for any value that includes a date and a time.
- Use `DateTimeOffset.UtcNow` when generating server-side timestamps.
- Do not use `DateTime` for instants in domain models, DTOs, integration events, persistence entities, or API responses.

#### Date-only values
- Use `DateOnly` for values that are truly date-only.
- Do not represent a date-only value as `DateTime` or `DateTimeOffset` “at midnight”.

#### “Now” usage
- Prefer passing `DateTimeOffset now` into domain behavior when feasible (testability), but this is optional and can be adopted incrementally.

### 3) Persistence

#### PostgreSQL
- For instants: use `timestamptz` (timestamp with time zone) to store UTC instants.
- Ensure values are written in UTC and read back as UTC (i.e., `Offset == 00:00`).

#### Azure Table Storage
- Table entities commonly use `DateTimeOffset? Timestamp` (platform-managed).
- All custom instant fields must be treated as UTC when written/read.

#### MongoDB
- Mongo stores instants as UTC under the hood.
- Persist instants as `DateTimeOffset` in the model where possible; avoid `DateTime` in domain/public contracts.

### 4) Contracts (HTTP + integration events)
- All instants in public DTOs and integration events must be `DateTimeOffset`.
- JSON representation must be ISO-8601 with an offset (prefer `Z`/UTC).
- Never emit/accept “unspecified kind” timestamps.

### 5) Imports (Sessionize and other external sources)
- If an upstream value has **no offset/timezone**, treat it as UTC.
- Convert to `DateTimeOffset` immediately at the boundary.

## Inventory (current state)
This section lists known types/fields that currently carry date/time values, and highlights places that still use `DateTime`.

### Shared
- `src/Shared/HexMaster.Attendr.Core/DomainModels/DomainModel.cs`: `CreatedOn`/`ModifiedOn` (`DateTimeOffset`) ✅
- `src/Shared/HexMaster.Attendr.Core/DomainModels/StatefulDomainModel.cs`: `CreatedOn`/`ModifiedOn` (`DateTimeOffset`) ✅
- `src/Shared/HexMaster.Attendr.Core/DomainEvents/DomainEvent.cs`: `OccurredAt` (`DateTimeOffset`) ✅
- `src/Shared/HexMaster.Attendr.IntegrationEvents/Events/IntegrationEvent.cs`: `OccurredAt` (`DateTime`) ⚠️
- `src/Shared/HexMaster.Attendr.IntegrationEvents/Events/Profiles/ProfileCheckedInEvent.cs`: `StartDateTime`/`EndDateTime` (`DateTime`) ⚠️
- `src/Shared/HexMaster.Attendr.IntegrationEvents/Events/Conferences/PresentationUpdatedEvent.cs`: `StartDateTime`/`EndDateTime` (`DateTimeOffset`) ✅
- `src/Shared/HexMaster.Attendr.IntegrationEvents/Events/Conferences/PresentationScheduleChangeEvent.cs`: `StartDateTime`/`EndDateTime` (`DateTimeOffset`) ✅

### Conferences
- `src/Conferences/HexMaster.Attendr.Conferences/DomainModels/Presentation.cs`: `StartDateTime`/`EndDateTime` (`DateTimeOffset`) ✅
- `src/Conferences/HexMaster.Attendr.Conferences.Abstractions/Dtos/PresentationDto.cs`: `StartDateTime`/`EndDateTime` (`DateTimeOffset`) ✅
- `src/Conferences/HexMaster.Attendr.Conferences.Data.Postgres/Entities/PresentationEntity.cs`: `StartDateTime`/`EndDateTime` (`DateTimeOffset`) ✅
- `src/Conferences/HexMaster.Attendr.Conferences/DomainModels/Topic.cs`: `createdOn` (`DateTime`) ⚠️ (timestamp semantics; should become `DateTimeOffset`)
- `src/Conferences/HexMaster.Attendr.Conferences/Services/SessionizeSyncService.cs`: imports Sessionize times and converts to `DateTimeOffset` assuming UTC ✅

### Groups
- `src/Groups/HexMaster.Attendr.Groups/DomainModels/CheckIn.cs`: `StartDateTime`/`EndDateTime` (`DateTime`) ⚠️
- `src/Groups/HexMaster.Attendr.Groups.Abstractions/Dtos/CheckInDto.cs`: `StartDateTime`/`EndDateTime` (`DateTime`) ⚠️
- `src/Groups/HexMaster.Attendr.Groups.Data.Postgress/Entities/CheckInEntity.cs`: `StartDateTime`/`EndDateTime` (`DateTime`) ⚠️
- `src/Groups/HexMaster.Attendr.Groups.Abstractions/DomainModels/IPresentationData.cs`: `StartDateTime`/`EndDateTime` (`DateTime`) ⚠️
- Group invitation/join request timestamps: `DateTimeOffset` ✅

### Presence
- `src/Presence/HexMaster.Attendr.Presence/DomainModels/PresentationPresence.cs`: `StartDateTime`/`EndDateTime` (`DateTime`) ⚠️; `CheckedInAt` (`DateTimeOffset?`) ✅
- `src/Presence/HexMaster.Attendr.Presence.Data.Postgres/Entities/PresentationPresenceEntity.cs`: `StartDateTime`/`EndDateTime` (`DateTime`) ⚠️; `CheckedInAt` (`DateTimeOffset?`) ✅
- `src/Presence/HexMaster.Attendr.Presence.Abstractions/Dtos/PresentationToRateDto.cs`: `StartDateTime`/`EndDateTime` (`DateTime`) ⚠️
- Responses with `StartDate`/`EndDate` as `DateTime` ⚠️ (likely date-only and should become `DateOnly`)

### Notifications
- Table Storage entities:
  - `src/Notifications/HexMaster.Attendr.Notifications.Data.TableStorage/Entities/NotificationEntity.cs`: `CreatedAt`, `ExpiresAt`, etc. (`DateTime`) ⚠️
  - `src/Notifications/HexMaster.Attendr.Notifications.Data.TableStorage/Entities/PushSubscriptionEntity.cs`: `CreatedAt`, `UpdatedAt`, etc. (`DateTime`) ⚠️
  - `src/Notifications/HexMaster.Attendr.Notifications.Data.TableStorage/Entities/NotificationPreferencesEntity.cs`: `CreatedAt`, `UpdatedAt`, `DoNotDisturbUntil` (`DateTime`) ⚠️
- Domain models and abstractions:
  - `src/Notifications/HexMaster.Attendr.Notifications/DomainModels/*.cs`: several `DateTime` properties (`CreatedAt`, `DeliveredAt`, `ExpiresAt`, `DoNotDisturbUntil`, etc.) ⚠️
  - `src/Notifications/HexMaster.Attendr.Notifications.Abstractions/**/*.cs`: several `DateTime` in public contracts ⚠️

### Profiles
- `src/Profiles/HexMaster.Attendr.Profiles.Abstractions/Dtos/ProfileTopicOccasionDto.cs`: `Date` (`DateTimeOffset`) ✅
- Table Storage models use `DateTimeOffset` for `CreatedOn`/`ModifiedOn` ✅

## UX / behavior (frontend)
- The frontend must display instants in the browser’s locale and timezone.
- No manual offset math in the UI; rely on ISO-8601 timestamps with offset (`Z`) from the backend.

## API / contracts
- Any new/changed endpoints or integration events must use `DateTimeOffset` for instants.
- When changing an existing DTO/event from `DateTime` → `DateTimeOffset`, treat it as a breaking contract unless versioned.

## Domain & data
- Domain models should model time with the smallest correct type:
  - `DateOnly` for date-only
  - `DateTimeOffset` for date+time instants/scheduled times

## Observability
- All logs/metrics/traces that include time values should use UTC timestamps and/or rely on the logging system’s timestamping.

## Security
- Ensure no auth decisions depend on local time conversions.
- Avoid trusting client-provided timestamps for security-critical logic.

## Acceptance criteria
- A new spec exists describing the conventions listed in this document.
- The spec includes a “current state” inventory with at least the known `DateTime` hotspots.
- The spec is linked from `specs/index.md`.

## Test plan
- Spec-only change (no code changes).

## Rollout
- Adopt conventions for all new work immediately.
- Create follow-up tasks/specs per service to migrate remaining `DateTime` usages.

## Open questions
- Should we introduce versioned integration events/DTOs for `DateTime` → `DateTimeOffset` migrations?
- For presence/conference schedule “date-only” responses, should contracts use `DateOnly` or `DateTimeOffset` (at UTC midnight) for compatibility with current clients?
