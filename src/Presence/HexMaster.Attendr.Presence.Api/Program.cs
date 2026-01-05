using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Conferences.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.IntegrationEvents.Extensions;
using HexMaster.Attendr.Presence.Data.MongoDb.Extensions;
using HexMaster.Attendr.Presence.Api.Features.GetMyConferences;
using HexMaster.Attendr.Presence.Api.Features.RatePresentation;
using HexMaster.Attendr.Presence.Api.Features.CreateConferencePresence;
using HexMaster.Attendr.Presence.Api.Features.UpdatePresentation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

// Register shared cache client
builder.Services.AddAttendrCache(builder.Configuration);

// Register integration services
builder.Services.AddProfilesIntegration(builder.Configuration);
builder.Services.AddConferencesIntegration(builder.Configuration);

// Register Presence module services
builder.Services.AddMongoDbPresenceRepository(builder.Configuration);
builder.Services.AddIntegrationEvents(builder.Configuration);
builder.Services.AddDaprSidekick();
builder.Services.AddDaprClient();

// Register feature slice services
builder.Services.AddScoped<HexMaster.Attendr.Presence.Api.Features.CreateConferencePresence.CreateConferencePresenceService>();
builder.Services.AddScoped<HexMaster.Attendr.Presence.Api.Features.UpdatePresentation.UpdatePresentationService>();
builder.Services.AddScoped<HexMaster.Attendr.Presence.Api.Features.RatePresentation.GetRandomPresentationToRateService>();
builder.Services.AddScoped<HexMaster.Attendr.Presence.Api.Features.RatePresentation.RatePresentationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

// Map feature slice endpoints
app.MapGetMyConferencesEndpoint();
app.MapGetRandomPresentationToRateEndpoint();
app.MapRatePresentationEndpoint();

// Map event handler endpoints
app.MapProfileFollowedConferenceEventHandler();
app.MapProfilesFollowedConferenceEventHandler();
app.MapPresentationUpdatedEventHandler();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();

