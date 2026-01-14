namespace HexMaster.Attendr.IntegrationEvents.Constants;

/// <summary>
/// Contains constants for integration event topic names used across the Attendr application.
/// These topic names are used as Dapr pub/sub topics for inter-service communication.
/// </summary>
public static class IntegrationEventTopics
{
    /// <summary>
    /// Topic for conference created events.
    /// Published when a new conference is created.
    /// </summary>
    public const string ConferenceCreated = "conference.created";

    /// <summary>
    /// Topic for conference updated events.
    /// Published when conference details are updated.
    /// </summary>
    public const string ConferenceUpdated = "conference.updated";

    /// <summary>
    /// Topic for profile created events.
    /// Published when a new user profile is created.
    /// </summary>
    public const string ProfileCreated = "profile.created";

    /// <summary>
    /// Topic for profile updated events.
    /// Published when profile details are updated.
    /// </summary>
    public const string ProfileUpdated = "profile.updated";

    /// <summary>
    /// Topic for profile followed conference events.
    /// Published when a single profile follows a conference.
    /// </summary>
    public const string ProfileFollowedConference = "profile.followed.conference";

    /// <summary>
    /// Topic for profiles followed conference events (bulk).
    /// Published when multiple profiles follow a conference (e.g., through a group).
    /// </summary>
    public const string ProfilesFollowedConference = "profiles.followed.conference";

    /// <summary>
    /// Topic for presentation updated events.
    /// Published when presentation details are updated during conference synchronization.
    /// </summary>
    public const string PresentationUpdated = "presentation.updated";

    /// <summary>
    /// Topic for presentation schedule change events.
    /// Published when a presentation schedule changes and a profile has favorited it.
    /// </summary>
    public const string PresentationScheduleChanged = "presentation.schedule-changed";

    /// <summary>
    /// Topic for profile checked in events.
    /// Published when a profile checks in or out of a presentation.
    /// </summary>
    public const string ProfileCheckedIn = "profile.checked-in";

    /// <summary>
    /// Topic for profile conference attendance changed events.
    /// Published when a profile changes their attendance status for a conference.
    /// </summary>
    public const string ProfileConferenceAttendanceChanged = "profile.conference-attendance-changed";

    /// <summary>
    /// Topic for group member added events.
    /// Published when a member is added to a group.
    /// </summary>
    public const string GroupMemberAdded = "group.member-added";

    /// <summary>
    /// Topic for group member removed events.
    /// Published when a member is removed from a group.
    /// </summary>
    public const string GroupMemberRemoved = "group.member-removed";
}
