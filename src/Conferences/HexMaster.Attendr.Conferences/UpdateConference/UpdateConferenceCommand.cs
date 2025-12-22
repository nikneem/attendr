using HexMaster.Attendr.Conferences.Abstractions.Dtos;

namespace HexMaster.Attendr.Conferences.UpdateConference;

/// <summary>
/// Command to update an existing conference.
/// </summary>
/// <param name="Id">The unique identifier of the conference to update.</param>
/// <param name="Title">The updated title of the conference.</param>
/// <param name="City">The updated city where the conference is held.</param>
/// <param name="Country">The updated country where the conference is held.</param>
/// <param name="ImageUrl">The updated URL of the conference image.</param>
/// <param name="StartDate">The updated start date of the conference.</param>
/// <param name="EndDate">The updated end date of the conference.</param>
/// <param name="SynchronizationSource">Optional synchronization source information.</param>
public sealed record UpdateConferenceCommand(
    Guid Id,
    string Title,
    string City,
    string Country,
    string? ImageUrl,
    DateOnly StartDate,
    DateOnly EndDate,
    SynchronizationSourceDto? SynchronizationSource);
