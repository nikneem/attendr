# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Attendr is a conference management platform with a .NET 10 microservices backend, Angular 21 frontend, and .NET Aspire orchestration. Authentication uses Auth0 (OIDC/JWT).

## Running the Application

The preferred way to run everything is via Aspire:
```bash
aspire run
```
This starts all backend services, databases (PostgreSQL, Redis), and the Angular frontend. Only restart if `apphost.cs` changes.

To run a single backend service:
```bash
dotnet run --project src/Conferences/HexMaster.Attendr.Conferences.Api
```

Frontend dev server:
```bash
cd src/App && npm start
# Runs at http://localhost:4200
```

## Build and Test Commands

```bash
# Build entire .NET solution
dotnet build src/Attendr.slnx

# Run all .NET tests
dotnet test src/Attendr.slnx

# Run a single service's tests
dotnet test src/Groups/HexMaster.Attendr.Groups.Tests

# Frontend build and test
cd src/App && npm run build
cd src/App && npm test
```

## Architecture

### Backend: Microservices with CQRS

Five independent services, each with its own API, domain logic, data layer, and tests:
- **Conferences** — conference/session/topic management, Sessionize sync, AI-powered topic analysis (Semantic Kernel)
- **Groups** — user groups, membership, join requests
- **Profiles** — user profiles
- **Presence** — session attendance, check-ins, ratings
- **Notifications** — push notifications, preferences (Azure Table Storage)

Plus a **Proxy API** (YARP reverse proxy gateway) and **Aspire AppHost** for orchestration.

### Per-Service Project Structure

Each service follows this layered pattern:
```
{Service}/
  HexMaster.Attendr.{Service}.Abstractions/  # DTOs, shared interfaces
  HexMaster.Attendr.{Service}/               # Domain models, Features/, Observability/
  HexMaster.Attendr.{Service}.Api/            # Program.cs, Endpoints/, Authorization/
  HexMaster.Attendr.{Service}.Data.Postgres/  # Repository implementations, migrations
  HexMaster.Attendr.{Service}.Tests/          # xUnit tests with Moq and Bogus
```

### CQRS Pattern (Features-based)

Business logic lives in `Features/` folders organized by use case. Each feature folder contains:
- A **Query** or **Command** record (e.g., `GetConferenceQuery`)
- A **Handler** class implementing `IQueryHandler<TQuery, TResult>` or `ICommandHandler<TCommand, TResult>` from `HexMaster.Attendr.Core.CommandHandlers`

Handlers are registered manually in each service's `ServiceCollectionExtensions.cs`. There is no MediatR — DI wiring is explicit.

### API Layer

APIs use **Minimal API endpoints** organized in `Endpoints/` classes with static `Map*Endpoints()` extension methods called from `Program.cs`. Each service includes `EventHandlersEndpoints` for Dapr pub/sub integration.

### Inter-Service Communication

Services communicate via **Dapr** pub/sub using integration events defined in `Shared/HexMaster.Attendr.IntegrationEvents/`. Each service subscribes to events through `EventHandlersEndpoints`.

### Shared Libraries

- `Shared/HexMaster.Attendr.Core/` — CQRS interfaces, cache keys, pagination constants (default: 25, max: 100), observability helpers, configuration utilities
- `Shared/HexMaster.Attendr.IntegrationEvents/` — Cross-service event contracts

### Observability

Each service has an `Observability/` folder with custom `ActivitySources` and `*Metrics` classes using OpenTelemetry. Query/command handlers instrument operations with tracing and metrics.

### Data

- **PostgreSQL**: Conferences, Groups, Profiles, Presence (via Npgsql)
- **Azure Table Storage**: Notifications, Profile data
- **Redis**: Caching layer
- **MongoDB**: Presence (legacy, being migrated to Postgres)

### Frontend: Angular 21

- **Standalone components** throughout (no NgModules)
- **Signal-based state management** using stores in `shared/stores/`
- **Services** in `shared/services/` handle HTTP communication
- **PrimeNG 21** with Aura theme for UI components
- **OIDC auth** via `angular-auth-oidc-client`
- Pages split into `pages/public/` and `pages/private/`

## Conventions

- **Commits**: Conventional Commits format — `feat(scope):`, `fix(scope):`, etc.
- **Versioning**: GitVersion with semantic versioning; use `+semver: minor` or `+semver: major` in merge commit messages for version bumps
- **Branches**: `feature/`, `fix/`, `docs/`, `refactor/`, `test/` prefixes
- **Testing**: xUnit with Moq for mocking and Bogus for test data generation. Arrange-Act-Assert pattern. Test files mirror the Features/ structure.
- **Infrastructure**: Azure Bicep templates in `Infrastructure/bicep/`

## MCP Servers

Three MCP servers are configured (`.mcp.json`):
- **aspire** — orchestration diagnostics (resource status, logs, traces)
- **hexmaster-design-guidelines** — project ADRs and coding guidelines; consult before architectural changes
- **playwright** — browser automation for functional testing
