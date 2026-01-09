namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing a presentation speaker.
/// </summary>
public interface IPresentationSpeaker
{
    /// <summary>
    /// Gets the unique identifier of the speaker.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the speaker.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the optional profile picture URL of the speaker.
    /// </summary>
    string? ProfilePictureUrl { get; }
}
