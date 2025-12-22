namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// Request DTO for creating a new conference.
/// </summary>
/// <param name="Title">The title of the conference.</param>
/// <param name="City">The city where the conference is held (optional).</param>
/// <param name="Country">The country where the conference is held (optional).</param>
/// <param name="ImageUrl">Optional URL to a conference image or logo.</param>
/// <param name="StartDate">The start date of the conference.</param>
/// <param name="EndDate">The end date of the conference.</param>
/// <param name="SynchronizationSource">Optional synchronization source configuration.</param>
public sealed record CreateConferenceRequest(
    string Title,
    string? City,
    string? Country,
    string? ImageUrl,
    DateOnly StartDate,
    DateOnly EndDate,
    SynchronizationSourceDto? SynchronizationSource);
