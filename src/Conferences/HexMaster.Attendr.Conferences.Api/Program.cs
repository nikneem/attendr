using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.Conferences.Api.Endpoints;
using HexMaster.Attendr.Conferences.Data.MongoDb.Extensions;
using HexMaster.Attendr.Conferences.Extensions;
using HexMaster.Attendr.IntegrationEvents.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Core.Cache.Extensions;
using Sessionize.Api.Client.DependencyInjection;
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
            .AddSource(ActivitySources.Conferences.Name)
            .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("HexMaster.Attendr.Conferences")
            .AddOtlpExporter();
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.AddOtlpExporter();
    logging.IncludeFormattedMessage = true;
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

// Register repositories and services
builder.Services.AddMongoDbConferenceRepository(builder.Configuration);
builder.Services.AddAttendrConferencesServices();
builder.Services.AddSessionizeApiClient();
builder.Services.AddIntegrationEvents(builder.Configuration);
builder.Services.AddProfilesIntegration(builder.Configuration);
builder.Services.AddAttendrCache(builder.Configuration);
#if DEBUG
builder.Services.AddDaprSidekick();
#endif
builder.Services.AddDaprClient();

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
app.MapConferencesEndpoints();
app.MapConferencesIntegrationEndpoints();
app.MapEventHandlersEndpoints();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
