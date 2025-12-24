# Conferences Integration Library

This library provides integration with the Conferences API for other services within the Attendr platform.

## Features

- **Cache-Aside Pattern**: Automatically caches conference details to improve performance
- **HTTP Client Integration**: Built-in HTTP client configuration
- **Logging Support**: Comprehensive logging for diagnostics
- **Centralized Configuration**: Uses the centralized Attendr configuration

## Installation

Add a project reference to `HexMaster.Attendr.Conferences.Integrations`:

```xml
<ProjectReference Include="..\HexMaster.Attendr.Conferences.Integrations\HexMaster.Attendr.Conferences.Integrations.csproj" />
```

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "Attendr": {
    "Integration": {
      "Profiles": "https://localhost:7001",
      "Groups": "https://localhost:7002",
      "Conferences": "https://localhost:7003"
    }
  }
}
```

For local development:

```json
{
  "Attendr": {
    "Integration": {
      "Profiles": "http://localhost:5001",
      "Groups": "http://localhost:5002",
      "Conferences": "http://localhost:5003"
    }
  }
}
```

## Registration

In your `Program.cs`, register the service:

```csharp
using HexMaster.Attendr.Conferences.Integrations.Extensions;
using HexMaster.Attendr.Core.Cache.Extensions;

// Register cache client (required dependency)
builder.Services.AddAttendrCache(builder.Configuration);

// Register conferences integration
builder.Services.AddConferencesIntegration(builder.Configuration);
```

## Usage

Inject `IConferencesIntegrationService` into your classes:

```csharp
using HexMaster.Attendr.Conferences.Integrations.Abstractions;

public class MyService
{
    private readonly IConferencesIntegrationService _conferencesIntegration;

    public MyService(IConferencesIntegrationService conferencesIntegration)
    {
        _conferencesIntegration = conferencesIntegration;
    }

    public async Task<ConferenceDetailsDto?> GetConference(Guid conferenceId)
    {
        // This will:
        // 1. Check cache first
        // 2. If not in cache, fetch from API
        // 3. Store result in cache for 30 minutes
        // 4. Return the conference details
        return await _conferencesIntegration.GetConferenceDetails(conferenceId);
    }
}
```

## API Endpoint

The integration library calls the anonymous integration endpoint:

- **Endpoint**: `GET /api/conferences-integration/{id}`
- **Authentication**: Anonymous (for internal service-to-service communication)
- **Response**: `ConferenceDetailsDto` or 404 if not found
- **OpenAPI**: Excluded from documentation

## Caching

- **Cache Key Pattern**: `conference:details:{id}`
- **Cache TTL**: 30 minutes
- **Cache Store**: Dapr shared state store
- **Pattern**: Cache-aside (lazy loading)

## Error Handling

The service will throw exceptions for:
- Network errors (HttpRequestException)
- Server errors (HTTP 500+)

Returns `null` for:
- Conference not found (HTTP 404)

## Configuration

The service uses the centralized `AttendrConfiguration` from the Core project. The base URL for the Conferences service is retrieved from `Attendr:Integration:Conferences` in the configuration.
