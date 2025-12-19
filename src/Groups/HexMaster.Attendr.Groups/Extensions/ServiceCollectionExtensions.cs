using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Groups.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrGroupsServices(this IServiceCollection services)
    {
        // TODO: Register command handlers when implemented
        // services.AddScoped<ICommandHandler<CreateGroupCommand, CreateGroupResult>, CreateGroupCommandHandler>();

        // TODO: Register repositories when implemented
        // services.AddScoped<IGroupRepository, GroupRepository>();

        return services;
    }
}
