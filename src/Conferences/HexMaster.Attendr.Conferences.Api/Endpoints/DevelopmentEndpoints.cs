using HexMaster.Attendr.Conferences.Abstractions.Dtos;
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
        IConferenceRepository repository,
        ICommandHandler<CreateConferenceCommand, CreateConferenceResult> handler,
        CancellationToken cancellationToken)
    {
        // Check if there are any conferences in the database
        var (existingConferences, totalCount) = await repository.ListConferencesAsync(
            null,
            1,
            1,
            showHidden: true,
            currentProfileId: null,
            cancellationToken);

        if (totalCount > 0)
        {
            return Results.Ok(new { message = "Conferences already exist in the database. Skipping seed operation.", count = totalCount });
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        var futureTechCommand = new CreateConferenceCommand(
            Title: "Future Tech '26",
            City: "Utrecht",
            Country: "The Netherlands",
            ImageUrl: "https://futuretech.nl/wp-content/uploads/2025/11/Futuretech-2026-diap.png",
            StartDate: DateOnly.Parse("2026-03-10"),
            EndDate: DateOnly.Parse("2026-03-11"),
            SynchronizationSource: new SynchronizationSourceDto("Sessionize", "4vfzhv8l"),
            CreatedByProfileId: Guid.NewGuid());

        var dummyCommand = new CreateConferenceCommand(
            Title: $"Tech United Extreme '{DateTime.Now.Year}",
            City: "Den Haag",
            Country: "The Netherlands",
            ImageUrl: null,
            StartDate: today,
            EndDate: tomorrow,
            SynchronizationSource: null,
            CreatedByProfileId: Guid.NewGuid());

        var dnfCommand = new CreateConferenceCommand(
            Title: $"DotnetFriday",
            City: "Nieuwegein",
            Country: "The Netherlands",
            ImageUrl: "https://dotnetfriday.nl/images/logo.jpg",
            StartDate: DateOnly.Parse("2026-04-10"),
            EndDate: DateOnly.Parse("2026-04-10"),
            SynchronizationSource: new SynchronizationSourceDto("Sessionize", "l1ueluvh"),
            CreatedByProfileId: Guid.NewGuid());




        var result = await handler.Handle(futureTechCommand, cancellationToken);
        result = await handler.Handle(dummyCommand, cancellationToken);
        result = await handler.Handle(dnfCommand, cancellationToken);
        return Results.Created($"/api/conferences/{result.Id}", result);
    }
}