# Copilot Instructions

## Pre-change protocol
- Assess the codebase and explicitly list assumptions before every change.
- Consult the hexmaster-design-guidelines MCP server for documentation before making any change.
- Make a task list and confirm the tasks with the user before proceeding.
- Use Aspire hosting and client libraries wherever possible.

## Architecture placement rules
- Abstractions project: hold service and repository abstractions, DTOs, domain model interfaces, enums, and value objects.
- Service project: hold domain models, features, and services.
- IntegrationEvents project: hold all integration events.
- Core project: hold code shared across multiple services (modules).
- ServiceDefaults project: hold shared startup and plumbing code.

## API and endpoints
- Configure endpoints in dedicated endpoint files and use MapGroup when convenient.

## Feature slices
- Always add backend features as slices.
- Commands: a DTO arrives, convert to a command, handle it, and return a DTO.
- Queries: a DTO (with optional filters) arrives, convert to a query, pass to a repository, and return a DTO or list of DTOs.

## Frontend (Angular)
- Angular 21 with standalone components configuration.
- PrimeNG is the component library.
- The app is zoneless (does not use zone.js for change detection).
- Always use signals for state management and updates.
- Use computed signals for derived state.
- Use effect() for side effects when necessary.
- Avoid traditional Angular change detection patterns (ChangeDetectorRef, markForCheck, etc.).


