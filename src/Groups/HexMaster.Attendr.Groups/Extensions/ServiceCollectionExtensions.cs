using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Groups.ApproveJoinRequest;
using HexMaster.Attendr.Groups.DenyJoinRequest;
using HexMaster.Attendr.Groups.FollowConference;
using HexMaster.Attendr.Groups.GetGroupDetails;
using HexMaster.Attendr.Groups.GetGroupFollowedConferences;
using HexMaster.Attendr.Groups.GetMyGroups;
using HexMaster.Attendr.Groups.JoinGroup;
using HexMaster.Attendr.Groups.ListGroups;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.RemoveMember;
using HexMaster.Attendr.Groups.UnfollowConference;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Groups.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrGroupsServices(this IServiceCollection services)
    {
        // Register metrics
        services.AddSingleton<GroupMetrics>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetMyGroupsQuery, IReadOnlyCollection<MyGroupDto>>, GetMyGroupsQueryHandler>();
        services.AddScoped<IQueryHandler<ListGroupsQuery, ListGroupsResult>, ListGroupsQueryHandler>();
        services.AddScoped<IQueryHandler<GetGroupDetailsQuery, GroupDetailsDto?>, GetGroupDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetGroupFollowedConferencesQuery, IReadOnlyCollection<FollowedConferenceDto>>, GetGroupFollowedConferencesQueryHandler>();

        // Register command handlers
        services.AddScoped<ICommandHandler<JoinGroupCommand>, JoinGroupCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveMemberCommand>, RemoveMemberCommandHandler>();
        services.AddScoped<ICommandHandler<ApproveJoinRequestCommand>, ApproveJoinRequestCommandHandler>();
        services.AddScoped<ICommandHandler<DenyJoinRequestCommand>, DenyJoinRequestCommandHandler>();
        services.AddScoped<ICommandHandler<FollowConferenceCommand>, FollowConferenceCommandHandler>();
        services.AddScoped<ICommandHandler<UnfollowConferenceCommand>, UnfollowConferenceCommandHandler>();

        return services;
    }
}
