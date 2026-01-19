using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.Notifications.Api.Endpoints;
using HexMaster.Attendr.Notifications.Data.TableStorage.Extensions;
using HexMaster.Attendr.Notifications.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure App Configuration (Release mode only)
builder.Configuration.AddAttendrAzureAppConfiguration(builder.Environment.EnvironmentName);

builder.AddServiceDefaults();
builder.AddAzureTableServiceClient(AspireConstants.TableStorage.Notifications);
builder.AddAzureTableServiceClient(AspireConstants.TableStorage.NotificationPreferences);
builder.AddAzureTableServiceClient(AspireConstants.TableStorage.Subscriptions);

// Add OpenAPI
builder.Services.AddOpenApi();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://attendr.eu.auth0.com/";
        options.Audience = "https://api.attendr.com";
    });
builder.Services.AddAuthorization();

// Add Dapr
builder.Services.AddDaprClient();
builder.Services.AddProfilesIntegration(builder.Configuration);
builder.Services.AddAttendrCache(builder.Configuration);
builder.Services.AddTableStorageNotificationRepositories();
builder.Services.AddNotificationFeatures();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map notification endpoints
app.MapNotificationsEndpoints();
app.MapNotificationPreferencesEndpoints();
app.MapNotificationPreferencesDetailEndpoints();
app.MapNotificationTypesEndpoints();
app.MapEventHandlersEndpoints();
app.MapPushSubscriptionsEndpoints();

// Enable Dapr pub/sub
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
