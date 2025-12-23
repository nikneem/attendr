namespace HexMaster.Attendr.Groups.GetGroupDetails;

/// <summary>
/// DTO representing a conference followed by a group.
/// </summary>
/// <param name="Id">The unique identifier of the conference.</param>
/// <param name="Name">The name/title of the conference.</param>
/// <param name="Location">The location of the conference (city and country).</param>
/// <param name="StartDate">The start date of the conference.</param>
/// <param name="EndDate">The end date of the conference.</param>
public sealed record FollowedConferenceDto(
    Guid Id,
    string Name,
    string Location,
    DateOnly StartDate,
    DateOnly EndDate);
