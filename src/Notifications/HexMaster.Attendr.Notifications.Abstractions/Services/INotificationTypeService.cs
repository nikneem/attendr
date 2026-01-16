using HexMaster.Attendr.Notifications.Abstractions.Models;

namespace HexMaster.Attendr.Notifications.Abstractions.Services;

/// <summary>
/// Service for managing and retrieving notification type configurations.
/// </summary>
public interface INotificationTypeService
{
    /// <summary>
    /// Gets all available notification types.
    /// </summary>
    IReadOnlyList<INotificationType> GetAllTypes();

    /// <summary>
    /// Gets a specific notification type by its key.
    /// </summary>
    INotificationType? GetTypeByKey(string typeKey);

    /// <summary>
    /// Checks if a notification type exists.
    /// </summary>
    bool TypeExists(string typeKey);
}
