# Spec: Standardize API Errors with ProblemDetails

## Summary
Adopt consistent `application/problem+json` responses across Notifications endpoints so the Angular app can display actionable errors and logs/telemetry are easier to correlate.

## Context
- Some endpoints return `BadRequest<string>` (push subscription validation) in ../../src/Notifications/HexMaster.Attendr.Notifications.Api/Endpoints/PushSubscriptionsEndpoints.cs.
- Some endpoints return bodyless 404s.

## Goals
- Use `ProblemDetails` for 400/404/409 (and other failures where applicable).
- Include `traceId` for correlation.

## Non-goals
- No cross-solution exception middleware unification.

## Contract
- Content-Type: `application/problem+json`
- Fields:
  - `type`: stable identifier (e.g., `urn:attendr:errors:push-subscription-invalid`)
  - `title`: short summary
  - `status`: HTTP status
  - `detail`: user-facing explanation
  - `instance`: request path
  - `extensions.traceId`: current activity trace id

## Acceptance criteria
- Invalid push subscription returns 400 ProblemDetails.
- Notification not found returns 404 ProblemDetails (optional, but preferred for consistency).
- Preference concurrency conflict returns 409 ProblemDetails.

## Test plan
- API integration tests verify content-type and required fields.
