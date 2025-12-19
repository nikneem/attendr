# OpenTelemetry Implementation Guide

## Overview

All Attendr backend APIs are configured with OpenTelemetry (OTel) for comprehensive distributed tracing, metrics collection, and structured logging. This document describes the setup and usage patterns.

## Architecture

### ActivitySources

Each module has a dedicated `ActivitySource` for creating spans (activities):

- **Profiles API**: `HexMaster.Attendr.Profiles` - User profile operations
- **Groups API**: `HexMaster.Attendr.Groups` - Group management operations
- **Conferences API**: `HexMaster.Attendr.Conferences` - Conference operations
- **Proxy API**: `HexMaster.Attendr.Proxy` - Reverse proxy request routing

ActivitySources are centrally defined in [ActivitySources.cs](./src/Shared/HexMaster.Attendr.Core/Observability/ActivitySources.cs).

### Configuration

Each API is configured with:

1. **Tracing Instrumentation**:
   - ASP.NET Core instrumentation (HTTP requests)
   - HttpClient instrumentation (outbound requests)
   - Custom ActivitySources for business logic

2. **Metrics Collection**:
   - ASP.NET Core metrics (request count, latency, error rates)
   - HttpClient metrics (outbound request metrics)

3. **Logging**:
   - Structured logging via `ILogger<T>`
   - OpenTelemetry log exporter with formatted messages and scopes

4. **OTLP Exporter**:
   - Exports traces, metrics, and logs to OTLP endpoint
   - Default endpoint: `http://localhost:4317` (gRPC)
   - Configurable via `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable

## Configuration

### Appsettings

Each API's `appsettings.json` includes OpenTelemetry configuration:

```json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317",
  "OTEL_SERVICE_NAME": "profiles-api",
  "OTEL_SERVICE_VERSION": "1.0.0"
}
```

These values can be overridden via environment variables:

```bash
# OTLP gRPC endpoint (required for local observability backend)
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

# Service identification
export OTEL_SERVICE_NAME=profiles-api
export OTEL_SERVICE_VERSION=1.0.0
```

### Environment Variables

```bash
# OTLP Exporter (all signals)
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

# Or signal-specific endpoints
OTEL_EXPORTER_OTLP_TRACES_ENDPOINT=http://jaeger:4317
OTEL_EXPORTER_OTLP_METRICS_ENDPOINT=http://prometheus:4317
OTEL_EXPORTER_OTLP_LOGS_ENDPOINT=http://loki:4317

# Service identification
OTEL_SERVICE_NAME=profiles-api
OTEL_SERVICE_VERSION=1.0.0

# Sampling (optional)
OTEL_TRACES_SAMPLER=always_on       # 100% sampling
OTEL_TRACES_SAMPLER=traceidratio    # Configurable percentage
OTEL_TRACES_SAMPLER_ARG=0.1         # 10% sampling
```

## Usage Examples

### Creating Activities (Spans)

Activities are created using `ActivitySource.StartActivity()`:

```csharp
using System.Diagnostics;
using HexMaster.Attendr.Core.Observability;

public sealed class CreateProfileCommandHandler : ICommandHandler<CreateProfileCommand, CreateProfileResult>
{
    public async Task<CreateProfileResult> Handle(CreateProfileCommand command, CancellationToken cancellationToken)
    {
        // Create an activity for this operation
        using var activity = ActivitySources.Profiles.StartActivity("CreateProfile", ActivityKind.Internal);
        
        // Add tags for correlation and filtering
        activity?.SetTag("profile.subject_id", command.SubjectId);
        activity?.SetTag("profile.email", command.Email);

        try
        {
            // ... business logic ...
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

### Structured Logging

Use `ILogger<T>` for structured logging with trace context correlation:

```csharp
public sealed class CreateProfileCommandHandler : ICommandHandler<CreateProfileCommand, CreateProfileResult>
{
    private readonly ILogger<CreateProfileCommandHandler> _logger;

    public CreateProfileCommandHandler(ILogger<CreateProfileCommandHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateProfileResult> Handle(CreateProfileCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating profile for subject {SubjectId}", command.SubjectId);
        
        try
        {
            // ... business logic ...
            _logger.LogInformation("Profile created successfully with ID {ProfileId}", profileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create profile for subject {SubjectId}", command.SubjectId);
            throw;
        }
    }
}
```

### Nested Activities

For multi-step operations, create nested activities:

```csharp
using var activity = ActivitySources.Profiles.StartActivity("CreateProfile", ActivityKind.Internal);

try
{
    // Step 1: Validate
    using (var validateActivity = ActivitySources.Profiles.StartActivity("ValidateProfile"))
    {
        validateActivity?.SetTag("validation.rules_checked", 5);
        // validation logic
    }

    // Step 2: Persist
    using (var persistActivity = ActivitySources.Profiles.StartActivity("PersistProfile"))
    {
        // persistence logic
    }

    activity?.SetStatus(ActivityStatusCode.Ok);
}
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    activity?.RecordException(ex);
    throw;
}
```

## Observability Backends

### Local Development with Aspire Dashboard

The simplest local option is .NET Aspire Dashboard:

```bash
# Install Aspire Dashboard
dotnet tool install -g aspire-dashboard

# Run Aspire Dashboard on default ports
aspire-dashboard

# APIs will export to localhost:4317 automatically
```

Visit http://localhost:18888 to view traces, metrics, and logs in real-time.

### Production: Jaeger + Prometheus + Loki

For production deployments, use a complete observability stack:

```bash
# Docker Compose example
docker-compose up jaeger prometheus loki grafana

# Configure environment variables
export OTEL_EXPORTER_OTLP_TRACES_ENDPOINT=http://jaeger:4317
export OTEL_EXPORTER_OTLP_METRICS_ENDPOINT=http://prometheus:4317
export OTEL_EXPORTER_OTLP_LOGS_ENDPOINT=http://loki:4317
```

### Cloud Providers

- **Azure Monitor**: Set `OTEL_EXPORTER_OTLP_ENDPOINT` to Azure Monitor OTLP endpoint
- **Datadog**: Use Datadog OTel exporters
- **New Relic**: Use New Relic OTel exporters
- **Honeycomb**: Use Honeycomb OTLP endpoint

## Best Practices

### ✅ DO

1. **Create activities for command handlers** - Each handler should have a top-level activity
2. **Add semantic tags** - Use domain-specific tags (e.g., `profile.subject_id`, not `id`)
3. **Log at the right level**:
   - `LogInformation` for success path events
   - `LogWarning` for recoverable errors
   - `LogError` with exception for failures
4. **Record exceptions** - Call `activity?.RecordException(ex)` for errors
5. **Use low-cardinality dimensions** - Tags should have bounded values (e.g., status codes, not user IDs)
6. **Inject ILogger** - Always take `ILogger<T>` in constructors

### ❌ DON'T

1. **Don't create activities for trivial operations** - Keep span count reasonable
2. **Don't use high-cardinality tags** - Avoid user IDs, email addresses, UUIDs as tag values
3. **Don't log PII** - Never include passwords, tokens, or sensitive data
4. **Don't create activities in loops** - Use counters instead for repeated operations
5. **Don't ignore sampling** - For high-traffic services, use sampling to reduce data volume

## Semantic Conventions

Follow OpenTelemetry semantic conventions for consistent naming:

### HTTP Tags

```csharp
activity?.SetTag("http.method", "GET");
activity?.SetTag("http.status_code", 200);
activity?.SetTag("http.route", "/profiles/{id}");
```

### Database Tags

```csharp
activity?.SetTag("db.system", "azure_table_storage");
activity?.SetTag("db.operation", "create");
activity?.SetTag("db.name", "profiles");
```

### Custom (Domain-Specific) Tags

```csharp
activity?.SetTag("profile.subject_id", subjectId);
activity?.SetTag("profile.email", email);
activity?.SetTag("profile.action", "created");
```

Reference: https://opentelemetry.io/docs/specs/semconv/

## Querying Traces

### In Aspire Dashboard

1. Navigate to http://localhost:18888
2. Click **Traces** tab
3. Filter by service name, operation name, or status code
4. Click a trace to see detailed span waterfall

### In Jaeger UI

```bash
# Jaeger runs on port 6831 (Thrift) and 6832 (compact)
# UI is at http://localhost:16686

# Query examples:
# - service.name="profiles-api"
# - operation="CreateProfile"
# - status=ERROR
```

### Using Docker Compose

```yaml
version: '3'
services:
  jaeger:
    image: jaegertracing/all-in-one
    ports:
      - "6831:6831/udp"
      - "16686:16686"
    environment:
      COLLECTOR_OTLP_ENABLED: "true"

  prometheus:
    image: prom/prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml

  loki:
    image: grafana/loki
    ports:
      - "3100:3100"

  grafana:
    image: grafana/grafana
    ports:
      - "3000:3000"
    environment:
      GF_SECURITY_ADMIN_PASSWORD: admin
```

## Troubleshooting

### Traces not appearing

1. Verify `OTEL_EXPORTER_OTLP_ENDPOINT` is set correctly
2. Check that observability backend is running: `curl -s http://localhost:4317/` 
3. Review application logs for exporter errors
4. Ensure ActivitySource is registered: `ActivitySources.Profiles.StartActivity()` should work

### Missing logs

1. Verify `builder.Logging.AddOpenTelemetry()` is called in Program.cs
2. Check log level: `LogInformation` and above are sent by default
3. Ensure logs are reaching the exporter: add a console exporter for debugging

### High cardinality tags

If metrics show exploding label combinations:
1. Review tags being set in activities
2. Avoid user IDs, UUIDs, or email addresses as tag values
3. Use counters or histograms for high-cardinality dimensions

## References

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
- [.NET OpenTelemetry Guide](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
- [Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)
- [ADR 0008: Adopt OpenTelemetry](../../../docs/adrs/0008-adopt-opentelemetry-for-observability.md)
