namespace HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// Represents the severity level of a group activity.
/// </summary>
public enum ActivitySeverity
{
    Low = 0,
    Medium = 1,
    High = 2
}

/// <summary>
/// Abstract base class for group activity types.
/// Each activity type has an ID, severity level, and translation key for internationalization.
/// </summary>
public abstract class GroupActivityType
{
    public static readonly GroupActivityType ProfileJoinedGroup = new GroupActivityTypeProfileJoinedGroup();
    public static readonly GroupActivityType ProfileLeftGroup = new GroupActivityTypeProfileLeftGroup();
    public static readonly GroupActivityType ProfilePresentationCheckedIn = new GroupActivityTypeProfilePresentationCheckedIn();
    public static readonly GroupActivityType ProfilePresentationCheckedOut = new GroupActivityTypeProfilePresentationCheckedOut();
    public static readonly GroupActivityType ProfileAttendingConference = new GroupActivityTypeProfileAttendingConference();
    public static readonly GroupActivityType ProfileLeavingConference = new GroupActivityTypeProfileLeavingConference();

    public abstract int ActivityTypeId { get; }
    public abstract ActivitySeverity Severity { get; }
    public abstract string TranslationKey { get; }

    /// <summary>
    /// Gets a GroupActivityType by its ID.
    /// </summary>
    /// <param name="activityTypeId">The activity type ID.</param>
    /// <returns>The corresponding GroupActivityType.</returns>
    /// <exception cref="ArgumentException">Thrown when the activity type ID is invalid.</exception>
    public static GroupActivityType FromId(int activityTypeId)
    {
        return activityTypeId switch
        {
            1 => ProfileJoinedGroup,
            2 => ProfileLeftGroup,
            3 => ProfilePresentationCheckedIn,
            4 => ProfilePresentationCheckedOut,
            5 => ProfileAttendingConference,
            6 => ProfileLeavingConference,
            _ => throw new ArgumentException($"Invalid activity type ID: {activityTypeId}", nameof(activityTypeId))
        };
    }
}

public sealed class GroupActivityTypeProfileJoinedGroup : GroupActivityType
{
    public override int ActivityTypeId => 1;
    public override ActivitySeverity Severity => ActivitySeverity.Medium;
    public override string TranslationKey => "Groups.Activities.ProfileJoinedGroup";
}

public sealed class GroupActivityTypeProfileLeftGroup : GroupActivityType
{
    public override int ActivityTypeId => 2;
    public override ActivitySeverity Severity => ActivitySeverity.Low;
    public override string TranslationKey => "Groups.Activities.ProfileLeftGroup";
}

public sealed class GroupActivityTypeProfilePresentationCheckedIn : GroupActivityType
{
    public override int ActivityTypeId => 3;
    public override ActivitySeverity Severity => ActivitySeverity.High;
    public override string TranslationKey => "Groups.Activities.ProfilePresentationCheckedIn";
}

public sealed class GroupActivityTypeProfilePresentationCheckedOut : GroupActivityType
{
    public override int ActivityTypeId => 4;
    public override ActivitySeverity Severity => ActivitySeverity.Low;
    public override string TranslationKey => "Groups.Activities.ProfilePresentationCheckedOut";
}

public sealed class GroupActivityTypeProfileAttendingConference : GroupActivityType
{
    public override int ActivityTypeId => 5;
    public override ActivitySeverity Severity => ActivitySeverity.High;
    public override string TranslationKey => "Groups.Activities.ProfileAttendingConference";
}

public sealed class GroupActivityTypeProfileLeavingConference : GroupActivityType
{
    public override int ActivityTypeId => 6;
    public override ActivitySeverity Severity => ActivitySeverity.Low;
    public override string TranslationKey => "Groups.Activities.ProfileLeavingConference";
}