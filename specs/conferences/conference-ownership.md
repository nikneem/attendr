# Spec: Conference ownership

## Summary

When a conference is created, the `profileId` of the creating user is stored as the conference owner (`CreatedByProfileId`). A conference is always visible to its owner regardless of the global `IsVisible` flag. The owner can — at a later stage — edit all content (speakers, rooms, and presentations) of that conference. The owner cannot change the conference visibility directly; visibility is controlled by the system (e.g., toggled to `false` on manual edits, as per the manual content management spec).

## Context

- **Current behavior:** No explicit ownership concept is modelled on the conference. The Sessionize-sync flow is the main way conferences get populated; there is no enforcement of who may edit a conference beyond admin checks.
- **Problem / user pain:**
  - A conference creator has no guaranteed way to always see their own conference (it could be invisible and not show up in listings).
  - Ownership is implicit and not enforced in authorization; only `admin:attendr` permission guards write access.
  - The "who created this conference" information exists on the aggregate (referenced in the manual content management spec as `Conference.CreatedByProfileId`) but is not reliably populated or enforced at creation time.
- **Related existing code/docs:**
  - `specs/conferences/manual-conference-content-management.md` — references `Conference.CreatedByProfileId` and the creator/admin access model.
  - `Conferences` service aggregate in `src/Conferences/`.

## Goals

- Capture `profileId` as `CreatedByProfileId` on the `Conference` aggregate at creation time.
- Always include conferences owned by the requesting user in conference listings, even when `IsVisible = false`.
- Allow the owner to perform full CRUD on speakers, rooms, and presentations (aligned with the manual content management spec).
- Forbid the owner from toggling `IsVisible` directly; visibility changes are exclusively system-driven.

## Non-goals

- Transferring ownership from one profile to another.
- Co-ownership / multiple owner support.
- Admin override of ownership.
- Conference deletion by the owner (separate concern).
- Any Sessionize-sync changes.

## Users & scenarios

- **Primary user:** A conference organizer who creates a conference and expects it to always be accessible to them.
- **Key scenarios:**
  1. User creates a conference → their `profileId` is stored as `CreatedByProfileId`.
  2. User fetches the conference list → their own conferences appear regardless of `IsVisible`.
  3. Owner navigates to the edit page → they can manage speakers, rooms, and presentations (see manual content management spec).
  4. Owner tries to set `IsVisible` via the API → request is rejected with `403 Forbidden`.

## UX / behavior

- **My conferences section:** In the conference list UI, conferences owned by the current user that are currently invisible should still be shown (e.g., with a "Unpublished" badge or muted styling) so the owner can find and edit them.
- **Edit access:** The edit page (`/app/conferences/:id/edit`) is accessible to the owner without any special admin role.
- **Visibility control:** No visibility toggle control is exposed to the owner in the UI. The UI may display the current visibility state as read-only. A tooltip or note explains that visibility is managed by the system.
- **Errors & empty states:** If a non-owner, non-admin user attempts to access the edit page, show "Access denied". If the conference is not found, show "Not found".
- **Accessibility:** Status badges (e.g., "Unpublished") must have sufficient color contrast and an accessible text label (not color alone).

## API / contracts

### Create conference — capture owner

- **Endpoint:** `POST /api/conferences`
- The service resolves the authenticated user's `profileId` at creation time (via `ProfilesIntegration` or the JWT sub claim mapped to a profile).
- `CreatedByProfileId` is persisted on the `Conference` record.
- Not included in any request body; derived server-side only.

### List conferences — owner visibility override

- **Endpoint:** `GET /api/conferences`
- Current filter: `WHERE IsVisible = true`.
- New filter: `WHERE IsVisible = true OR CreatedByProfileId = @currentProfileId`.
- The query handler receives the (optional) `currentProfileId` and applies the override.
- Unauthenticated requests continue to receive only visible conferences.

### Get single conference — owner visibility override

- **Endpoint:** `GET /api/conferences/{conferenceId}`
- Returns the conference if `IsVisible = true` OR `CreatedByProfileId = @currentProfileId`.
- Returns `404 Not Found` to all other callers when `IsVisible = false`, to avoid information leakage.

### Block visibility changes by owner

- **Endpoints affected:** any endpoint that updates conference properties (e.g., `PUT /api/conferences/{conferenceId}`).
- If the request body includes an `IsVisible` field and the caller is the owner (not admin), respond with `403 Forbidden` and a `ProblemDetails` body:
  ```json
  {
    "type": "https://attendr.dev/errors/forbidden-visibility-change",
    "title": "Visibility cannot be changed directly.",
    "status": 403,
    "detail": "Conference visibility is managed by the system. Owners cannot change it manually."
  }
  ```
- Admins retain the ability to change `IsVisible`.

### Backwards compatibility

- `CreatedByProfileId` will be `null` for conferences created before this change is deployed. Those conferences will not benefit from the owner-visibility override until the field is populated (see Rollout section).
- All existing API response shapes are unchanged; `CreatedByProfileId` is an internal field not exposed in public DTOs unless explicitly added.

## Domain & data

### Domain model changes

- `Conference` aggregate gains (if not already present):
  - `CreatedByProfileId: Guid?` — set once at creation, immutable.
- Add a `Create(…, Guid createdByProfileId)` factory overload (or update existing) that sets this field.
- Enforce immutability: attempting to change `CreatedByProfileId` after creation throws a `DomainException`.

### Persistence changes

- Add column `created_by_profile_id UUID NULL` to the `conferences` table.
- New EF Core migration: `AddConferenceOwner`.
- Index on `(created_by_profile_id)` to support efficient owner-filtered queries.

### Retention / privacy notes

- `CreatedByProfileId` references a profile; if a profile is deleted, the column should be set to `NULL` (or the conference becomes ownerless). This cascade behavior is out of scope for this spec but should be tracked as a follow-up.

## Observability

- **Traces:** Existing conference creation and query activities should include a `conference.created_by_profile_id` tag where available.
- **Metrics:** No new metrics required for this spec.
- **Logs:** Log a warning when a conference is returned to an owner despite `IsVisible = false`, at `Debug` level, to aid troubleshooting.

## Security

- **AuthN/AuthZ:**
  - Owner identity is resolved server-side from the authenticated JWT; never trust a client-supplied owner claim.
  - The owner check (profile ID match) is evaluated in the CQRS handler or a dedicated authorization handler — not in the endpoint.
  - Admins (`admin:attendr` permission) bypass the owner check for all operations except the visibility-change block (admins retain visibility access).
- **Abuse considerations:**
  - Prevent enumeration: a non-owner/non-admin always receives `404` for invisible conferences, not `403`, to avoid confirming the existence of the resource.

## Acceptance criteria

- [ ] `POST /api/conferences` stores the authenticated user's `profileId` as `CreatedByProfileId`.
- [ ] `GET /api/conferences` returns invisible conferences to their owner and only visible conferences to all other callers.
- [ ] `GET /api/conferences/{id}` returns an invisible conference to its owner; returns `404` to all other callers for invisible conferences.
- [ ] `PUT /api/conferences/{id}` (or equivalent update endpoint) returns `403` with a `ProblemDetails` body when a non-admin owner attempts to set `IsVisible`.
- [ ] Owner can access and mutate speakers, rooms, and presentations via the edit API (per manual content management spec) without requiring admin role.
- [ ] Conferences with `CreatedByProfileId = NULL` (legacy data) are unaffected — no visibility override is applied.
- [ ] The Angular conference list shows unpublished owner conferences with an "Unpublished" indicator.
- [ ] No visibility toggle is shown to the owner in the Angular edit UI.

## Test plan

- **Unit:**
  - `Conference.Create(…)` sets `CreatedByProfileId` correctly and throws when called with an empty GUID.
  - Attempting to mutate `CreatedByProfileId` on an existing aggregate throws `DomainException`.
  - List query handler returns owner conferences when `IsVisible = false` and caller is owner.
  - Get single handler returns the conference for owner regardless of `IsVisible`; returns `null`/not-found for others.
  - Update handler rejects `IsVisible` changes from owner (non-admin).

- **Integration:**
  - End-to-end: create conference as user A, set invisible, query as user A → appears; query as user B → does not appear.
  - Attempt to set `IsVisible` as owner → `403`.
  - Attempt to set `IsVisible` as admin → succeeds.

- **Frontend:**
  - Conference list renders "Unpublished" badge on owner-only invisible conferences.
  - Edit page renders without visibility toggle for owner.
  - Non-owner is redirected / shown "Access denied" on the edit page.

## Rollout

- **Migration steps:**
  1. Deploy database migration `AddConferenceOwner` (nullable column, no downtime).
  2. Deploy backend with updated create/query logic.
  3. Deploy Angular frontend changes.
  4. Optionally backfill `created_by_profile_id` for existing conferences via a one-time script using audit logs or manual assignment.
- **Feature flag (if needed):** Not required; the owner-visibility override degrades gracefully for legacy rows (`CreatedByProfileId = NULL`).

## Open questions

- Should `CreatedByProfileId` ever be exposed in the public conference DTO? (e.g., to show "Organized by <name>" on the detail page.) Track as a follow-up.
- Cascade behavior when a profile is deleted: set `NULL`, block deletion, or make conferences ownerless? Define in a future privacy/data-retention spec.
- Should the owner be able to see a "Make visible" button after the system sets `IsVisible = false`? Currently out of scope; revisit when the publication workflow is designed.
