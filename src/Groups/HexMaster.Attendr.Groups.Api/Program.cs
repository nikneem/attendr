using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Conferences.Integrations.Extensions;
using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.Groups.Api.Endpoints;
using HexMaster.Attendr.Groups.Data.Postgress.Extensions;
using HexMaster.Attendr.Groups.Extensions;
using HexMaster.Attendr.IntegrationEvents.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure App Configuration (Release mode only)
builder.Configuration.AddAttendrAzureAppConfiguration(builder.Environment.EnvironmentName);

builder.AddServiceDefaults();
builder.AddAzureNpgsqlDataSource(connectionName: AspireConstants.Postgres.GroupsDatabase);
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
builder.Services.AddAttendrGroupsServices();
builder.Services.AddPostgresGroupRepository();
builder.Services.AddIntegrationEvents(builder.Configuration);

builder.Services.AddDaprClient();

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
app.MapGroupsEndpoints();
app.MapEventHandlersEndpoints();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
