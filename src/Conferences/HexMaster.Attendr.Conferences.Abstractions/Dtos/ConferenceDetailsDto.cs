namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// DTO for detailed conference information.
/// </summary>
/// <param name="Id">The conference identifier.</param>
/// <param name="Title">The title of the conference.</param>
/// <param name="City">The city where the conference is held.</param>
/// <param name="Country">The country where the conference is held.</param>
/// <param name="StartDate">The start date of the conference.</param>
/// <param name="EndDate">The end date of the conference.</param>
/// <param name="ImageUrl">Optional URL to a conference image or logo.</param>
/// <param name="SynchronizationSource">Optional synchronization source configuration.</param>
public sealed record ConferenceDetailsDto(
    Guid Id,
    string Title,
    string City,
    string Country,
    DateOnly StartDate,
    DateOnly EndDate,
    string? ImageUrl,
    SynchronizationSourceDto? SynchronizationSource);
