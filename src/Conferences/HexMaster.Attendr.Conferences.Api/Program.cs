using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Conferences.Api.Endpoints;
using HexMaster.Attendr.Conferences.Data.Postgres.Extensions;
using HexMaster.Attendr.Conferences.Extensions;
using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.IntegrationEvents.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddAuthorization();

// Add health checks
builder.Services.AddHealthChecks();

// Register repositories and services
builder.Services.AddAttendrConferencesServices();
builder.Services.AddSessionizeApiClient();
builder.Services.AddPostgresConferenceRepository();
builder.Services.AddDatabaseMigrations(); // Run migrations on startup
builder.Services.AddIntegrationEvents(builder.Configuration);
builder.Services.AddProfilesIntegration(builder.Configuration);
builder.Services.AddAttendrCache(builder.Configuration);
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

// Map endpoints
app.UseCors();
app.MapConferencesEndpoints();
app.MapConferencesIntegrationEndpoints();
app.MapEventHandlersEndpoints();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
