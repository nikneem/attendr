namespace HexMaster.Attendr.Notifications.Abstractions.Enums;

/// <summary>
/// Represents the severity level of a notification.
/// </summary>
public enum NotificationSeverity
{
    /// <summary>
    /// Informational notification.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Update notification about changes.
    /// </summary>
    Update = 1,

    /// <summary>
    /// Warning notification requiring attention.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error notification indicating a problem.
    /// </summary>
    Error = 3
}
