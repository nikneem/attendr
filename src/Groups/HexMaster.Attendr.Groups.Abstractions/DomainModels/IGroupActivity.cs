namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing an activity in a group.
/// </summary>
public interface IGroupActivity
{
    /// <summary>
    /// Gets the unique identifier for the activity.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the unique identifier of the profile that triggered the activity.
    /// </summary>
    Guid ProfileId { get; }

    /// <summary>
    /// Gets the date/time when the activity occurred.
    /// </summary>
    DateTimeOffset ActivityDate { get; }

    /// <summary>
    /// Gets the description of the activity.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the type of activity.
    /// </summary>
    GroupActivityType ActivityType { get; }
}
