using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Groups.GetGroupDetails;
using HexMaster.Attendr.Groups.GetMyGroups;
using HexMaster.Attendr.Groups.ListGroups;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Groups.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrGroupsServices(this IServiceCollection services)
    {
        // Register query handlers
        services.AddScoped<IQueryHandler<GetMyGroupsQuery, IReadOnlyCollection<MyGroupDto>>, GetMyGroupsQueryHandler>();
        services.AddScoped<IQueryHandler<ListGroupsQuery, ListGroupsResult>, ListGroupsQueryHandler>();
        services.AddScoped<IQueryHandler<GetGroupDetailsQuery, GroupDetailsDto?>, GetGroupDetailsQueryHandler>();

        return services;
    }
}
