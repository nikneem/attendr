using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Features.CreateConference;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

/// <summary>
/// Development-only endpoints that bypass authorization for local testing.
/// These endpoints are only mapped when the application runs in the Development environment.
/// </summary>
public static class DevelopmentEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dev/conferences")
            .WithName("ConferencesDevelopment")
            .WithTags("Development");

        group.MapPost("/seed", SeedDummyConference)
            .WithName("SeedDummyConference")
            .Produces<CreateConferenceResult>(StatusCodes.Status201Created)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> SeedDummyConference(
        ICommandHandler<CreateConferenceCommand, CreateConferenceResult> handler,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var command = new CreateConferenceCommand(
            Title: $"Dummy Conference {DateTime.Today:dd MMM yyyy}",
            City: "Amsterdam",
            Country: "Netherlands",
            ImageUrl: "https://futuretech.nl/wp-content/uploads/2025/11/Futuretech-2026-diap.png",
            StartDate: DateOnly.Parse("2026-03-10"),
            EndDate: DateOnly.Parse("2026-03-11"),
            SynchronizationSource: new SynchronizationSourceDto("Sessionize", "4vfzhv8l"),
            CreatedByProfileId: Guid.NewGuid());

        var result = await handler.Handle(command, cancellationToken);

        return Results.Created($"/api/conferences/{result.Id}", result);
    }
}
