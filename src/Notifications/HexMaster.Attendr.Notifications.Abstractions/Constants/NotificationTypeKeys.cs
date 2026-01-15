namespace HexMaster.Attendr.Notifications.Abstractions.Constants;

/// <summary>
/// Constants for notification type keys used throughout the system.
/// </summary>
public static class NotificationTypeKeys
{
    // Group notifications
    public const string GroupMemberAdded = "group.member-added";
    public const string GroupMemberRemoved = "group.member-removed";

    // Conference notifications
    public const string ConferenceCreated = "conference.created";
    public const string ConferenceUpdated = "conference.updated";
    public const string ProfileFollowedConference = "profile.followed-conference";

    // Presentation notifications
    public const string PresentationUpdated = "presentation.updated";
    public const string PresentationScheduleChanged = "presentation.schedule-changed";

    // Profile notifications
    public const string ProfileCreated = "profile.created";
    public const string ProfileUpdated = "profile.updated";

    // Check-in notifications
    public const string ProfileCheckedIn = "profile.checked-in";
    public const string ProfileConferenceAttendanceChanged = "profile.conference-attendance-changed";
}
