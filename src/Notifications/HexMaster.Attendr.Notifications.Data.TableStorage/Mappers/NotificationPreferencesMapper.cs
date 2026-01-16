using System.Text.Json;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Data.TableStorage.Entities;
using HexMaster.Attendr.Notifications.DomainModels;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Mappers;

/// <summary>
/// Maps between NotificationPreferences domain model and NotificationPreferencesEntity.
/// </summary>
internal static class NotificationPreferencesMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static NotificationPreferencesEntity ToEntity(NotificationPreferences preferences)
    {
        return new NotificationPreferencesEntity
        {
            PartitionKey = preferences.ProfileId.ToString(),
            RowKey = NotificationPreferencesEntity.PreferencesRowKey,
            ProfileId = preferences.ProfileId.ToString(),
            TypeChannelPreferencesJson = JsonSerializer.Serialize(preferences.TypeChannelPreferences, JsonOptions),
            DoNotDisturbUntil = preferences.DoNotDisturbUntil,
            CreatedAt = preferences.CreatedAt,
            UpdatedAt = preferences.UpdatedAt
        };
    }

    public static NotificationPreferences ToDomain(NotificationPreferencesEntity entity)
    {
        return new NotificationPreferences
        {
            ProfileId = Guid.Parse(entity.ProfileId),
            TypeChannelPreferences = JsonSerializer.Deserialize<Dictionary<string, Dictionary<NotificationChannel, bool>>>(
                entity.TypeChannelPreferencesJson, JsonOptions) ?? new Dictionary<string, Dictionary<NotificationChannel, bool>>(),
            DoNotDisturbUntil = entity.DoNotDisturbUntil,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
