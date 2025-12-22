using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.CreateConference;
using HexMaster.Attendr.Conferences.GetConference;
using HexMaster.Attendr.Conferences.ListConferences;
using HexMaster.Attendr.Core.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Conferences.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrConferencesServices(this IServiceCollection services)
    {


        // Register command handlers
        services.AddScoped<ICommandHandler<CreateConferenceCommand, CreateConferenceResult>, CreateConferenceCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<ListConferencesQuery, ListConferencesResult>, ListConferencesQueryHandler>();
        services.AddScoped<IQueryHandler<GetConferenceQuery, ConferenceDetailsDto?>, GetConferenceQueryHandler>();

        return services;
    }
}
