using Azure;
using Azure.Data.Tables;

namespace HexMaster.Attendr.Profiles.Data.TableStorage.Models;

internal sealed class ProfileTopicEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // ProfileId
    public string RowKey { get; set; } = string.Empty; // TopicKey
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Id { get; set; } = string.Empty;
    public string ProfileId { get; set; } = string.Empty;
    public string TopicKey { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string OccasionsJson { get; set; } = string.Empty;
    public bool IsManual { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
}
