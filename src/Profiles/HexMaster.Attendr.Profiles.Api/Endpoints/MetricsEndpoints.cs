using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Features.GetMetrics;

namespace HexMaster.Attendr.Profiles.Api.Endpoints;

public static class MetricsEndpoints
{
    public static IEndpointRouteBuilder MapMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/profiles/metrics", GetMetrics)
            .WithName("GetProfileMetrics")
            .Produces<ProfileMetricsDto>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetMetrics(
        IQueryHandler<GetMetricsQuery, ProfileMetricsDto> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetMetricsQuery();
        var result = await handler.Handle(query, cancellationToken);
        return Results.Ok(result);
    }
}
