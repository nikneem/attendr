using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Groups.GetMyGroups;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Groups.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrGroupsServices(this IServiceCollection services)
    {
        // Register query handlers
        services.AddScoped<IQueryHandler<GetMyGroupsQuery, IReadOnlyCollection<MyGroupDto>>, GetMyGroupsQueryHandler>();

        return services;
    }
}
