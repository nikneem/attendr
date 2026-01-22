namespace HexMaster.Attendr.IntegrationEvents.Models;

/// <summary>
/// Simple topic DTO (key/name) used in integration events.
/// </summary>
public sealed record PresentationTopicDto(string Key, string Name);
