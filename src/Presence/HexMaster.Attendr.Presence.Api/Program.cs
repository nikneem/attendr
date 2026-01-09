using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Conferences.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.IntegrationEvents.Extensions;
using HexMaster.Attendr.Presence.Data.MongoDb.Extensions;
using HexMaster.Attendr.Presence.Extensions;
using HexMaster.Attendr.Presence.Api.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Azure App Configuration (Release mode only)
builder.Configuration.AddAttendrAzureAppConfiguration(builder.Environment.EnvironmentName);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(ActivitySources.Presence.Name)
            .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("HexMaster.Attendr.Presence")
            .AddOtlpExporter();
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging
        .AddOtlpExporter()
        .IncludeFormattedMessage = true;
});

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = "https://attendr.eu.auth0.com/";
    options.Audience = "https://api.attendr.com";
});
builder.Services.AddAuthorization();

// Add health checks
builder.Services.AddHealthChecks();

// Register shared cache client
builder.Services.AddAttendrCache(builder.Configuration);

// Register integration services
builder.Services.AddProfilesIntegration(builder.Configuration);
builder.Services.AddConferencesIntegration(builder.Configuration);

// Register Presence module services
builder.Services.AddMongoDbPresenceRepository(builder.Configuration);
builder.Services.AddIntegrationEvents(builder.Configuration);
#if DEBUG
builder.Services.AddDaprSidekick();
#endif
builder.Services.AddDaprClient();

// Register feature slice services
builder.Services.AddPresenceFeatures();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Only returns the overall health status
});
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/startup");

// Map endpoints
app.MapPresenceEndpoints();
app.MapEventHandlersEndpoints();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();

