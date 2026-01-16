namespace HexMaster.Attendr.Notifications.Abstractions.Enums;

/// <summary>
/// Represents the channels through which notifications can be delivered.
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// In-application notification displayed in the UI.
    /// </summary>
    InApp = 0,

    /// <summary>
    /// Email notification.
    /// </summary>
    Email = 1,

    /// <summary>
    /// Push notification to mobile device or browser.
    /// </summary>
    Push = 2
}
