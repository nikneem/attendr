namespace HexMaster.Attendr.Conferences.Abstractions.Dtos;

/// <summary>
/// Result DTO for conference creation.
/// </summary>
/// <param name="Id">The unique identifier of the created conference.</param>
/// <param name="Title">The title of the conference.</param>
/// <param name="City">The city where the conference is held.</param>
/// <param name="Country">The country where the conference is held.</param>
/// <param name="StartDate">The start date of the conference.</param>
/// <param name="EndDate">The end date of the conference.</param>
public sealed record CreateConferenceResult(
    Guid Id,
    string Title,
    string City,
    string Country,
    DateOnly StartDate,
    DateOnly EndDate);
