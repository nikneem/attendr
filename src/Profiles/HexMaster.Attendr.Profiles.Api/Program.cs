using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.Core.Configuration;
using HexMaster.Attendr.Profiles.Api.Endpoints;
using HexMaster.Attendr.Profiles.Data.TableStorage.Extensions;
using HexMaster.Attendr.Profiles.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure App Configuration (Release mode only)
builder.Configuration.AddAttendrAzureAppConfiguration(builder.Environment.EnvironmentName);

builder.AddServiceDefaults();
builder.AddAzureTableServiceClient(AspireConstants.TableStorage.Profiles);

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
    .AddTableStorageProfileRepository();

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
app.MapProfileEndpoints();
app.MapProfilesIntegrationEndpoints();
app.MapVersionEndpoints();

app.Run();

