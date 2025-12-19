using Microsoft.AspNetCore.Authentication.JwtBearer;
using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Groups.Api.Endpoints;
using HexMaster.Attendr.Groups.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

// Register profiles integration service
builder.Services.AddProfilesIntegration(builder.Configuration);

// Register Groups module services
builder.Services.AddAttendrGroupsServices();

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
app.MapGroupsEndpoints();

app.Run();
