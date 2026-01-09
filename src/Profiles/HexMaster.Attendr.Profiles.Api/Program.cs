using Microsoft.AspNetCore.Authentication.JwtBearer;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.Profiles.Api.Endpoints;
using HexMaster.Attendr.Profiles.Data.TableStorage.Extensions;
using HexMaster.Attendr.Profiles.Extensions;
using Scalar.AspNetCore;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Azure App Configuration (Release mode only)
builder.Configuration.AddAttendrAzureAppConfiguration(builder.Environment.EnvironmentName);


builder.AddAzureTableServiceClient("profiles");

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

// Register Profiles module services
builder.Services
    .AddAttendrProfilesServices(builder.Configuration)
    .AddTableStorageProfileRepository(builder.Configuration);

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

// Map endpoints
app.MapProfileEndpoints();

app.Run();

