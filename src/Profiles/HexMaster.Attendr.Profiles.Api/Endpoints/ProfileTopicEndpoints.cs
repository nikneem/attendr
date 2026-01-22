using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.GetProfileTopics;

namespace HexMaster.Attendr.Profiles.Api.Endpoints;

public static class ProfileTopicEndpoints
{
    public static IEndpointRouteBuilder MapProfileTopicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profiles/{profileId}/topics")
            .WithName("ProfileTopics");

        group.MapGet("/", GetProfileTopics)
            .WithName("GetProfileTopics")
            .Produces<IReadOnlyList<ProfileTopicDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetProfileTopics(
        string profileId,
        IQueryHandler<GetProfileTopicsQuery, IReadOnlyList<ProfileTopicDto>> handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return Results.BadRequest(new { error = "profileId is required" });
        }

        var result = await handler.Handle(new GetProfileTopicsQuery(profileId.Trim()), cancellationToken);
        return Results.Ok(result);
    }
}
