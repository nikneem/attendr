using HexMaster.Attendr.Conferences.Abstractions.Dtos;

namespace HexMaster.Attendr.Conferences.CreateConference;

/// <summary>
/// Command to create a new conference.
/// </summary>
/// <param name="Title">The title of the conference.</param>
/// <param name="City">The city where the conference is held.</param>
/// <param name="Country">The country where the conference is held.</param>
/// <param name="StartDate">The start date of the conference.</param>
/// <param name="EndDate">The end date of the conference.</param>
/// <param name="SynchronizationSource">Optional synchronization source configuration.</param>
public sealed record CreateConferenceCommand(
    string Title,
    string City,
    string Country,
    DateOnly StartDate,
    DateOnly EndDate,
    SynchronizationSourceDto? SynchronizationSource);
