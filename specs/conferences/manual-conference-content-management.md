# Spec: Manual conference content management (speakers, rooms, presentations)

## Summary
Add first-class support for manually maintaining a conference’s speakers, rooms, and presentations.

- Frontend edit route: `/app/conferences/{conferenceId}/edit`.
- Only the conference creator (by profile ID) or an admin can access and mutate data.
- Backend adds conference-scoped CRUD APIs for speakers, rooms, and presentations.
- All APIs follow the existing CQRS + vertical slice pattern used by the Conferences service.
- Any manual content change forces the conference to become invisible (`IsVisible = false`).

## Context
Today, conference content (speakers/rooms/presentations) is effectively maintained via Sessionize import.
That blocks the “fix data quickly” and “create conferences without Sessionize” scenarios, and makes it hard to correct imported data.

The Conferences service already has:
- Conference aggregate with `Rooms`, `Speakers`, `Presentations`.
- Sessionize sync service that populates domain models.
- Admin authorization policy (`AuthorizationPolicies.Admin`) based on permission `admin:attendr`.

## Goals
- Enable creator/admin users to:
  - Create/update/delete speakers.
  - Create/update/delete rooms.
  - Create/update/delete presentations (including assigning room + speakers).
- Provide REST APIs under `/api/conferences/{conferenceId}/...` for these resources.
- Ensure all mutations are safe and consistent with the Conference aggregate invariants.
- Automatically set the conference to invisible on manual change.

## Non-goals
- Editing topics/AI analysis behavior.
- Drag-and-drop schedule UI.
- Conflict-free merging between Sessionize sync and manual edits.

## Users & scenarios
- Conference organizer (creator) creates a conference and manually adds sessions.
- Admin corrects imported speaker names/rooms.
- Organizer adds a late-breaking session and makes conference visible again only when ready.

## UX / behavior

### Route
- Add a private route: `/app/conferences/:id/edit`.
- Access control:
  - If the user is not creator/admin → show an “Access denied” state (or redirect to the conference details page).
  - If the conference does not exist → show “Not found”.

### Page layout
- Page title: “Edit conference”.
- Show conference header summary (title, city/country, dates, current visibility) read-only.
- Show **three tabs**:
  1) **Speakers**
  2) **Rooms**
  3) **Presentations**

### Tab behaviors
- Each tab shows:
  - A list of items for the conference.
  - A minimal create/edit form.
  - Delete action with confirmation.

#### Speakers tab
- List columns: name, company (if available), profile picture URL indicator.
- Create/edit form fields:
  - Name (required)
  - Company (optional)
  - Profile picture URL (optional)

#### Rooms tab
- List columns: name, capacity.
- Create/edit form fields:
  - Name (required)
  - Capacity (required, > 0)

#### Presentations tab
- List columns: title, start/end, room, speakers count.
- Create/edit form fields:
  - Title (required)
  - Abstract (required)
  - StartDateTime (required)
  - EndDateTime (required, must be after start)
  - Room (required; choose from rooms)
  - Speakers (required; multi-select from speakers, at least 1)

### Visibility note
- After any successful manual create/update/delete of speakers/rooms/presentations, show a warning message:
  - “Conference was set to invisible due to manual changes.”

## API / contracts

### Authorization model
All endpoints below require authentication and allow access if:
- The user has `AuthorizationPolicies.Admin`, OR
- The user’s profile ID matches `Conference.CreatedByProfileId`.

Implementation detail:
- Reuse `ProfilesIntegration` to resolve the current user to a profile.
- Add an owner check (in an authorization handler or in the CQRS handler) that verifies `CreatedByProfileId`.

### Speakers endpoints
Base: `/api/conferences/{conferenceId}/speakers`

- `GET /api/conferences/{conferenceId}/speakers`
  - Returns list of speakers.
- `GET /api/conferences/{conferenceId}/speakers/{speakerId}`
  - Returns a single speaker.
- `POST /api/conferences/{conferenceId}/speakers`
  - Creates a speaker.
- `PUT /api/conferences/{conferenceId}/speakers/{speakerId}`
  - Updates a speaker.
- `DELETE /api/conferences/{conferenceId}/speakers/{speakerId}`
  - Deletes a speaker.

DTOs (new, Conference-scoped; do not break existing `SpeakerDto`):
- `ConferenceSpeakerDto`: `Id`, `Name`, `Company`, `ProfilePictureUrl`, `ExternalId` (optional)
- `CreateConferenceSpeakerRequest`: `Name`, `Company?`, `ProfilePictureUrl?`
- `UpdateConferenceSpeakerRequest`: `Name`, `Company?`, `ProfilePictureUrl?`

### Rooms endpoints
Base: `/api/conferences/{conferenceId}/rooms`

- `GET /api/conferences/{conferenceId}/rooms`
- `GET /api/conferences/{conferenceId}/rooms/{roomId}`
- `POST /api/conferences/{conferenceId}/rooms`
- `PUT /api/conferences/{conferenceId}/rooms/{roomId}`
- `DELETE /api/conferences/{conferenceId}/rooms/{roomId}`

DTOs (new):
- `ConferenceRoomDto`: `Id`, `Name`, `Capacity`, `ExternalId` (optional)
- `CreateConferenceRoomRequest`: `Name`, `Capacity`
- `UpdateConferenceRoomRequest`: `Name`, `Capacity`

### Presentations endpoints
Base: `/api/conferences/{conferenceId}/presentations`

- `GET /api/conferences/{conferenceId}/presentations`
- `GET /api/conferences/{conferenceId}/presentations/{presentationId}`
- `POST /api/conferences/{conferenceId}/presentations`
- `PUT /api/conferences/{conferenceId}/presentations/{presentationId}`
- `DELETE /api/conferences/{conferenceId}/presentations/{presentationId}`

DTOs (new; do not break existing `PresentationDto`):
- `ConferencePresentationDto`:
  - `Id`, `Title`, `Abstract`, `StartDateTime`, `EndDateTime`,
  - `RoomId`, `RoomName`,
  - `SpeakerIds`, `Speakers` (optional expanded list),
  - `ExternalId` (optional)
- `CreateConferencePresentationRequest`:
  - `Title`, `Abstract`, `StartDateTime`, `EndDateTime`, `RoomId`, `SpeakerIds[]`
- `UpdateConferencePresentationRequest`:
  - `Title`, `Abstract`, `StartDateTime`, `EndDateTime`, `RoomId`, `SpeakerIds[]`

### Error handling
- `404` if conference or resource not found.
- `400` for validation failures (missing name, capacity <= 0, end <= start, empty speakers list, etc.).
- `403` if not creator/admin.

## CQRS / vertical slice structure
Add feature slices under `src/Conferences/HexMaster.Attendr.Conferences/Features/`.

Pattern per resource:
- `Features/<Resource>/List...` (Query + Handler + response DTO)
- `Features/<Resource>/Get...`
- `Features/<Resource>/Create...`
- `Features/<Resource>/Update...`
- `Features/<Resource>/Delete...`

Handlers:
- Load the `Conference` aggregate.
- Perform the domain operation.
- Set `conference.UpdateVisibility(false)` (or a dedicated domain method like `conference.MarkInvisibleDueToManualChanges()`).
- Persist via repository.

Endpoints:
- Add new minimal API endpoint classes similar to existing `ConferencesEndpoints`.
- Map under `/api/conferences/{conferenceId}/...`.

## Domain & data

### Conference creator ownership
When a conference is created, store the creator’s profile ID.

Changes:
- Domain: add `CreatedByProfileId` to `Conference`.
- Data: add `created_by_profile_id` (UUID) to the `conferences` table and `ConferenceEntity`.
- API: update the create conference flow to resolve the caller’s profile and pass it to the create command/handler.

### Manual change forces invisibility
Any manual CRUD on speakers/rooms/presentations must set `IsVisible = false`.

Rationale:
- Manual edits indicate the content may not be ready for public display.

## Observability
- Add tracing activities per handler using the existing Conferences `ActivitySources`.
- Emit structured logs for create/update/delete with `{ConferenceId}` and resource IDs.

## Security
- Enforce creator/admin authorization on every endpoint.
- Validate conferenceId scoping (speaker/room/presentation must belong to conference).

## Acceptance criteria
- A creator can open `/app/conferences/{conferenceId}/edit` and manage speakers/rooms/presentations.
- A non-creator non-admin cannot access the page or endpoints.
- Admin can access and manage all conferences.
- Manual change sets the conference to invisible.
- Backend exposes the required endpoints for speakers, rooms, and presentations.

## Test plan
- Unit tests:
  - Conference ownership check.
  - Domain invariants (presentation requires existing room, at least 1 speaker).
  - “Manual edit sets invisible” behavior.
- Handler tests (xUnit + Moq):
  - Each command/query loads conference and persists.
  - Forbidden when not creator/admin.

## Rollout
- No migration of existing data required beyond adding `CreatedByProfileId` column (backfill strategy required).

## Open questions
- Backfill: what value should `CreatedByProfileId` be for existing conferences?
- Sessionize sync: should it be disabled after manual edits to avoid overwriting manual changes, or should it remain active?
