using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Profiles.Api.Endpoints;
using HexMaster.Attendr.Profiles.Extensions;
using HexMaster.Attendr.Profiles.Data.TableStorage.Extensions;
using HexMaster.Attendr.Core.Cache.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(ActivitySources.Profiles.Name)
            .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
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

// Register Profiles module services
builder.Services
    .AddAttendrProfilesServices()
    .AddProfilesTableStorage(builder.Configuration);

builder.Services.AddDaprSidekick();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapProfileEndpoints();

app.Run();

