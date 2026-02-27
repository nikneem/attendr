using HexMaster.Attendr.Conferences.Abstractions.Dtos;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

/// <summary>
/// Integration endpoints for internal service-to-service communication.
/// These endpoints are anonymous and not exposed in OpenAPI documentation.
/// </summary>
public static class ConferencesIntegrationEndpoints
{
    /// <summary>
    /// Maps the conferences integration endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapConferencesIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences-integration")
            .ExcludeFromDescription();

        group.MapGet("/{id:guid}", GetConferenceDetails)
            .WithName("GetConferenceDetailsIntegration")
            .Produces<ConferenceDetailsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapGet("/{conferenceId:guid}/presentations/{presentationId:guid}", GetPresentationDetails)
            .WithName("GetPresentationDetailsIntegration")
            .Produces<PresentationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetConferenceDetails(
        Guid id,
        IConferenceRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var conference = await repository.GetDetailsByIdAsync(id, null, cancellationToken);

            if (conference is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(conference);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetPresentationDetails(
        Guid conferenceId,
        Guid presentationId,
        IConferenceRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var conference = await repository.GetDetailsByIdAsync(conferenceId, null, cancellationToken);

            if (conference is null)
            {
                return Results.NotFound();
            }

            var presentation = conference.Presentations.FirstOrDefault(p => p.Id == presentationId);

            if (presentation is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(presentation);
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
