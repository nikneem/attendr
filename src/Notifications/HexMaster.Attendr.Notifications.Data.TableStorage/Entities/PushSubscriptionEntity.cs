using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Data.Tables;

namespace HexMaster.Attendr.Notifications.Data.TableStorage.Entities;

/// <summary>
/// Azure Table Storage entity for push subscriptions.
/// PartitionKey: ProfileId
/// RowKey: SHA256 hash of the subscription endpoint
/// </summary>
internal sealed class PushSubscriptionEntity : ITableEntity
{
    public static string CreateRowKey(string endpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(endpoint));
        return Convert.ToHexString(hash);
    }

    public string PartitionKey { get; set; } = string.Empty; // ProfileId
    public string RowKey { get; set; } = string.Empty;       // Hashed endpoint
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ProfileId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTimeOffset? ExpirationTime { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
