using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Models;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.Constants;
using HexMaster.Attendr.Notifications.Models;

namespace HexMaster.Attendr.Notifications.Services;

/// <summary>
/// Implementation of INotificationTypeService that provides hard-coded notification types.
/// </summary>
public sealed class NotificationTypeService : INotificationTypeService
{
    private readonly Dictionary<string, NotificationType> _notificationTypes;

    public NotificationTypeService()
    {
        _notificationTypes = InitializeNotificationTypes();
    }

    public IReadOnlyList<INotificationType> GetAllTypes()
    {
        return _notificationTypes.Values.Cast<INotificationType>().ToList();
    }

    public INotificationType? GetTypeByKey(string typeKey)
    {
        return _notificationTypes.TryGetValue(typeKey, out var type) ? type : null;
    }

    public bool TypeExists(string typeKey)
    {
        return _notificationTypes.ContainsKey(typeKey);
    }

    private static Dictionary<string, NotificationType> InitializeNotificationTypes()
    {
        var types = new List<NotificationType>
        {
            // Group notifications
            new()
            {
                TypeKey = NotificationTypeKeys.GroupMemberAdded,
                DisplayName = "Group Member Added",
                Description = "Notifies when someone joins a group you're in",
                Severity = NotificationSeverity.Info,
                AllowsStacking = true,
                StackWindowSeconds = 3600, // 1 hour
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                MessageTemplate = "{count} user(s) joined your group"
            },
            new()
            {
                TypeKey = NotificationTypeKeys.GroupMemberRemoved,
                DisplayName = "Group Member Removed",
                Description = "Notifies when someone leaves a group you're in",
                Severity = NotificationSeverity.Info,
                AllowsStacking = true,
                StackWindowSeconds = 3600,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                MessageTemplate = "{count} user(s) left your group"
            },
            new()
            {
                TypeKey = NotificationTypeKeys.GroupAccessRequested,
                DisplayName = "Group Access Request",
                Description = "Notifies group owners when someone requests access to a private group",
                Severity = NotificationSeverity.Update,
                AllowsStacking = true,
                StackWindowSeconds = 86400, // 24 hours
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, true }
                },
                MessageTemplate = "Member request for group {groupName}"
            },

            // Conference notifications
            new()
            {
                TypeKey = NotificationTypeKeys.ConferenceCreated,
                DisplayName = "Conference Created",
                Description = "Notifies when a new conference is created",
                Severity = NotificationSeverity.Info,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                }
            },
            new()
            {
                TypeKey = NotificationTypeKeys.ConferenceUpdated,
                DisplayName = "Conference Updated",
                Description = "Notifies when conference details are updated",
                Severity = NotificationSeverity.Update,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                }
            },
            new()
            {
                TypeKey = NotificationTypeKeys.ConferencePresentationsImported,
                DisplayName = "Conference Imported",
                Description = "Notifies when conference presentations have been successfully imported",
                Severity = NotificationSeverity.Info,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                }
            },
            new()
            {
                TypeKey = NotificationTypeKeys.ProfileFollowedConference,
                DisplayName = "Following Conference",
                Description = "Notifies when you start following a conference",
                Severity = NotificationSeverity.Info,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                }
            },

            // Presentation notifications
            new()
            {
                TypeKey = NotificationTypeKeys.PresentationUpdated,
                DisplayName = "Presentation Updated",
                Description = "Notifies when a favorited presentation is updated",
                Severity = NotificationSeverity.Update,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, true }
                }
            },
            new()
            {
                TypeKey = NotificationTypeKeys.PresentationScheduleChanged,
                DisplayName = "Schedule Changed",
                Description = "Critical notification when a favorited presentation schedule changes",
                Severity = NotificationSeverity.Warning,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, true }
                }
            },

            // Profile notifications
            new()
            {
                TypeKey = NotificationTypeKeys.ProfileCreated,
                DisplayName = "Profile Created",
                Description = "Welcome notification when profile is created",
                Severity = NotificationSeverity.Info,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                }
            },
            new()
            {
                TypeKey = NotificationTypeKeys.ProfileUpdated,
                DisplayName = "Profile Updated",
                Description = "Notifies when your profile is updated",
                Severity = NotificationSeverity.Info,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                }
            },

            // Check-in notifications
            new()
            {
                TypeKey = NotificationTypeKeys.ProfileCheckedIn,
                DisplayName = "Check-In Confirmation",
                Description = "Confirms when you check in to a presentation",
                Severity = NotificationSeverity.Info,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                }
            },
            new()
            {
                TypeKey = NotificationTypeKeys.ProfileConferenceAttendanceChanged,
                DisplayName = "Attendance Status Changed",
                Description = "Notifies when your conference attendance status changes",
                Severity = NotificationSeverity.Update,
                AllowsStacking = false,
                DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                },
                AvailableChannels = new Dictionary<NotificationChannel, bool>
                {
                    { NotificationChannel.InApp, true },
                    { NotificationChannel.Email, false },
                    { NotificationChannel.Push, false }
                }
            }
        };

        return types.ToDictionary(t => t.TypeKey, t => t);
    }
}
