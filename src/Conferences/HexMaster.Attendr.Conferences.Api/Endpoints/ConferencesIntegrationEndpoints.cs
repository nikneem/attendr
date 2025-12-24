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

        return app;
    }

    private static async Task<IResult> GetConferenceDetails(
        Guid id,
        IConferenceRepository repository,
        CancellationToken cancellationToken)
    {
        try
        {
            var conference = await repository.GetDetailsByIdAsync(id, cancellationToken);
            
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
}
