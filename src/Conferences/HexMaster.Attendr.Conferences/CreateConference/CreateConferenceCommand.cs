using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.CreateConference;

/// <summary>
/// Command to create a new conference.
/// </summary>
/// <param name="Title">The title of the conference.</param>
/// <param name="City">The city where the conference is held.</param>
/// <param name="Country">The country where the conference is held.</param>
/// <param name="ImageUrl">Optional URL to a conference image or logo.</param>
/// <param name="StartDate">The start date of the conference.</param>
/// <param name="EndDate">The end date of the conference.</param>
/// <param name="SynchronizationSource">Optional synchronization source configuration.</param>
public sealed record CreateConferenceCommand(
    string Title,
    string City,
    string Country,
    string? ImageUrl,
    DateOnly StartDate,
    DateOnly EndDate,
    SynchronizationSourceDto? SynchronizationSource) : IAttendrCommand;
