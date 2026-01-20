# Copilot Instructions

## Pre-change protocol
- Assess the codebase and explicitly list assumptions before every change.
- Consult the hexmaster-design-guidelines MCP server for documentation before making any change.
- Make a task list and confirm the tasks with the user before proceeding.
- Use Aspire hosting and client libraries wherever possible.

## Architecture overview
- **Modular monolith** structure with separate services (Conferences, Groups, Profiles, Presence, Notifications).
- **Vertical slice architecture**: organize features by use case, not by technical layer.
- **CQRS pattern**: separate command handlers and query handlers with dedicated request/response DTOs.
- **Pragmatic DDD**: rich domain models with behavior, private setters, and protected state. Avoid anemic models.
- **.NET 10** as the target framework.
- **Minimal APIs** preferred over controllers.

## Project structure rules
Each service follows this structure:
- **Abstractions project**: service and repository abstractions, DTOs, domain model interfaces, enums, and value objects.
- **Service project**: domain models, features (vertical slices), and services.
- **Api project**: API endpoints using minimal APIs.
- **Data project**: repository implementations (e.g., Data.Postgres, Data.TableStorage, Data.MongoDb).
- **IntegrationEvents project**: all integration events for the service.
- **Tests project**: unit tests.
- **Core project** (shared): code shared across all services (ICommandHandler, IQueryHandler, base domain models).

## Vertical slice architecture
- Organize features by use case in dedicated folders under `/Features`.
- Each slice contains: Command/Query, Handler, DTOs, and tests.
- Example: `Features/GetConferenceAttendance/` contains query, handler, and response DTO.
- Handlers orchestrate domain operations but contain NO business logic.
- All business logic lives in domain models.

## Domain models
- Use `StatefulDomainModel<TId>` base class for entities that need state tracking (Created, Pristine, Modified, Deleted).
- Use `DomainModel<TId>` for simpler entities without state tracking.
- Domain models must have private setters and behavior methods to enforce invariants.
- Use factory methods (e.g., `Create()`, `Load()`) instead of public constructors.
- NO anemic models - domain logic belongs in entities, not services or handlers.
- Value objects only when they add value (multi-field concepts, interdependent validation, domain behavior).
- Avoid wrapping primitives in value objects (no `FirstName`, `OrderId` value objects).

## Command and query handlers
- All handlers implement `ICommandHandler<TCommand, TResult>` or `IQueryHandler<TQuery, TResult>` from Core project.
- Handlers are thin orchestrators: load aggregates, call domain methods, persist, return DTOs.
- NO business logic in handlers - it belongs in domain models.
- Handlers translate `DomainException` to appropriate responses.
- Register handlers with DI explicitly.

## API endpoints
- Use minimal APIs, not controllers.
- Organize endpoints in dedicated static classes under `/Endpoints` (e.g., `ProfileEndpoints.cs`).
- Use `MapGroup()` to group related endpoints.
- Keep endpoint methods thin - delegate to handlers immediately.
- Example pattern:
  ```csharp
  var group = app.MapGroup("/api/profiles").WithTags("Profiles");
  group.MapPost("/", CreateProfile);
  group.MapGet("/{id:guid}", GetProfile);
  ```

## Integration events
- All integration events inherit from `IntegrationEvent` base class.
- Events include `EventId`, `OccurredAt`, and `EventType`.
- Use Dapr pub/sub for event publishing via `IIntegrationEventPublisher`.
- Integration events are defined in separate `IntegrationEvents` projects per service.

## Observability
- Use OpenTelemetry for tracing, metrics, and logging.
- Create activities using `ActivitySources` for important operations.
- Record metrics using dedicated metrics classes (e.g., `PresenceMetrics`).
- Add structured logging with appropriate log levels.

## Testing
- Use xUnit, Moq, and Bogus for unit tests.
- Test domain models thoroughly to verify business rules.
- Test handlers to ensure orchestration is correct.
- Mock repository interfaces and dependencies.

## Frontend (Angular)
- Angular 21 with standalone components configuration.
- PrimeNG is the component library.
- The app is zoneless (does not use zone.js for change detection).
- Always use signals for state management and updates.
- Use computed signals for derived state.
- Use effect() for side effects when necessary.
- Avoid traditional Angular change detection patterns (ChangeDetectorRef, markForCheck, etc.).


