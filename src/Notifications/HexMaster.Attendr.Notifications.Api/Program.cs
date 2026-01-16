using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Notifications.Api.Endpoints;
using HexMaster.Attendr.Notifications.Data.TableStorage.Extensions;
using HexMaster.Attendr.Notifications.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Azure Table Storage using Aspire integration
builder.AddAzureTableClient(AspireConstants.TableStorage.Notifications);
builder.AddAzureTableClient(AspireConstants.TableStorage.NotificationPreferences);

// Add OpenAPI
builder.Services.AddOpenApi();

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://attendr.eu.auth0.com/";
        options.Audience = "https://api.attendr.com";
    });
builder.Services.AddAuthorization();

// Add Dapr
builder.Services.AddDaprClient();

// Add notification repositories and services
builder.Services.AddTableStorageNotificationRepositories();
builder.Services.AddNotificationFeatures();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

// Map notification endpoints
app.MapNotificationsEndpoints();
app.MapNotificationPreferencesEndpoints();
app.MapNotificationPreferencesDetailEndpoints();
app.MapNotificationTypesEndpoints();
app.MapEventHandlersEndpoints();

// Enable Dapr pub/sub
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
