using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Groups.Abstractions.Dtos;
using HexMaster.Attendr.Groups.Features.ApproveJoinRequest;
using HexMaster.Attendr.Groups.Features.DenyJoinRequest;
using HexMaster.Attendr.Groups.Features.FollowConference;
using HexMaster.Attendr.Groups.Features.GetGroupDetails;
using HexMaster.Attendr.Groups.Features.GetGroupFollowedConferences;
using HexMaster.Attendr.Groups.Features.GetMyGroups;
using HexMaster.Attendr.Groups.Features.GetGroupCheckIns;
using HexMaster.Attendr.Groups.Features.JoinGroup;
using HexMaster.Attendr.Groups.Features.ListGroups;
using HexMaster.Attendr.Groups.Features.RemoveMember;
using HexMaster.Attendr.Groups.Features.UnfollowConference;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Features.ProcessProfileCheckedIn;
using HexMaster.Attendr.Groups.Features.ProcessProfileConferenceAttendanceChanged;
using HexMaster.Attendr.Groups.Features.ProcessGroupMemberAdded;
using HexMaster.Attendr.Groups.Features.ProcessGroupMemberRemoved;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Groups.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrGroupsServices(this IServiceCollection services)
    {
        services.AddSingleton<GroupMetrics>();

        services.AddScoped<IQueryHandler<GetMyGroupsQuery, IReadOnlyCollection<MyGroupDto>>, GetMyGroupsQueryHandler>();
        services.AddScoped<IQueryHandler<ListGroupsQuery, ListGroupsResult>, ListGroupsQueryHandler>();
        services.AddScoped<IQueryHandler<GetGroupDetailsQuery, GroupDetailsDto?>, GetGroupDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetGroupFollowedConferencesQuery, IReadOnlyCollection<FollowedConferenceDto>>, GetGroupFollowedConferencesQueryHandler>();
        services.AddScoped<IQueryHandler<GetGroupCheckInsQuery, IReadOnlyCollection<CheckInDto>>, GetGroupCheckInsQueryHandler>();

        services.AddScoped<ICommandHandler<JoinGroupCommand>, JoinGroupCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveMemberCommand>, RemoveMemberCommandHandler>();
        services.AddScoped<ICommandHandler<ApproveJoinRequestCommand>, ApproveJoinRequestCommandHandler>();
        services.AddScoped<ICommandHandler<DenyJoinRequestCommand>, DenyJoinRequestCommandHandler>();
        services.AddScoped<ICommandHandler<FollowConferenceCommand>, FollowConferenceCommandHandler>();
        services.AddScoped<ICommandHandler<UnfollowConferenceCommand>, UnfollowConferenceCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessProfileCheckedInCommand>, ProcessProfileCheckedInCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessProfileConferenceAttendanceChangedCommand>, ProcessProfileConferenceAttendanceChangedCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessGroupMemberAddedCommand>, ProcessGroupMemberAddedCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessGroupMemberRemovedCommand>, ProcessGroupMemberRemovedCommandHandler>();

        return services;
    }
}
