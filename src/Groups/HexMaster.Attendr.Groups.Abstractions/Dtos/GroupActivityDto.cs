namespace HexMaster.Attendr.Groups.Abstractions.Dtos;

/// <summary>
/// DTO representing a group activity.
/// </summary>
/// <param name="Id">The unique identifier of the activity.</param>
/// <param name="ProfileId">The unique identifier of the profile that triggered this activity.</param>
/// <param name="CreatedAt">The timestamp when this activity was created.</param>
/// <param name="Description">The description of the activity.</param>
/// <param name="ActivityTypeId">The ID of the activity type.</param>
/// <param name="ActivitySeverity">The severity level of the activity (0=Low, 1=Medium, 2=High).</param>
/// <param name="TranslationKey">The translation key for internationalization.</param>
public sealed record GroupActivityDto(
    Guid Id,
    Guid ProfileId,
    DateTimeOffset CreatedAt,
    string Description,
    int ActivityTypeId,
    int ActivitySeverity,
    string TranslationKey);
