using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Features.GetMetrics;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class MetricsEndpoints
{
    public static IEndpointRouteBuilder MapMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/conferences/metrics", GetMetrics)
            .WithName("GetConferenceMetrics")
            .Produces<ConferenceMetricsDto>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetMetrics(
        IQueryHandler<GetMetricsQuery, ConferenceMetricsDto> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetMetricsQuery();
        var result = await handler.Handle(query, cancellationToken);
        return Results.Ok(result);
    }
}
