using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Api.BackgroundServices;
using HexMaster.Attendr.Conferences.Api.Endpoints;
using HexMaster.Attendr.Conferences.Data.Postgres.Extensions;
using HexMaster.Attendr.Conferences.Extensions;
using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.IntegrationEvents.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using Sessionize.Api.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure App Configuration (Release mode only)
builder.Configuration.AddAttendrAzureAppConfiguration(builder.Environment.EnvironmentName);

builder.AddServiceDefaults();
builder.AddAzureNpgsqlDataSource(connectionName: AspireConstants.Postgres.ConferencesDatabase);

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

// Configure authorization with custom policies
builder.Services.AddAuthorizationBuilder()
    // Authenticated policy (explicit)
    .AddPolicy(AuthorizationPolicies.Authenticated, policy =>
        policy.RequireAuthenticatedUser())
    // Admin policy - requires admin:attendr permission
    .AddPolicy(AuthorizationPolicies.Admin, policy =>
        policy.Requirements.Add(new PermissionRequirement(Permissions.AdminAttendr)));

// Register custom authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://817f713k-4200.euw.devtunnels.ms",
            "http://localhost:4200",
            "https://localhost:4200",
            "https://attendr.live",
            "https://www.attendr.live",
            "https://*.attendr.live")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Register repositories and services
builder.Services.AddAttendrConferencesServices();
builder.Services.AddSemanticKernelForConferences(builder.Configuration);
builder.Services.AddSessionizeApiClient();
builder.Services.AddPostgresConferenceRepository();
builder.Services.AddDatabaseMigrations(); // Run migrations on startup
builder.Services.AddIntegrationEvents(builder.Configuration);
builder.Services.AddProfilesIntegration(builder.Configuration);
builder.Services.AddAttendrCache(builder.Configuration);
builder.Services.AddDaprClient();

// Register the background sync service
builder.Services.AddHostedService<ConferenceSyncBackgroundService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapConferencesEndpoints();
app.MapTopicsEndpoints();
app.MapConferencesIntegrationEndpoints();
app.MapEventHandlersEndpoints();
app.MapMetricsEndpoints();
app.MapVersionEndpoints();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
