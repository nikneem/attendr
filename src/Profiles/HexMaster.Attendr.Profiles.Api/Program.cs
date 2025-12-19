using HexMaster.Attendr.Profiles.Api.Endpoints;
using HexMaster.Attendr.Profiles.Extensions;
using HexMaster.Attendr.Profiles.Data.TableStorage.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddAuthentication()
    .AddJwtBearer();
builder.Services.AddAuthorization();

// Register Profiles module services
builder.Services
    .AddAttendrProfilesServices()
    .AddProfilesTableStorage(builder.Configuration);

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

