using HexMaster.Attendr.Aspire.AppHost;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Conferences.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.IntegrationEvents.Extensions;
using HexMaster.Attendr.Presence.Extensions;
using HexMaster.Attendr.Presence.Api.Endpoints;
using HexMaster.Attendr.Presence.Data.Postgres.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure App Configuration (Release mode only)
builder.Configuration.AddAttendrAzureAppConfiguration(builder.Environment.EnvironmentName);

builder.AddServiceDefaults();
builder.AddAzureNpgsqlDataSource(connectionName: AspireConstants.Postgres.PresenceDatabase);
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
builder.Services.AddPostgresPresenceRepositories();
// Register Presence module services
builder.Services.AddIntegrationEvents(builder.Configuration);
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

app.UseCors();
app.MapPresenceEndpoints();
app.MapEventHandlersEndpoints();
app.MapVersionEndpoints();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();

