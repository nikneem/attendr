namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing presentation data.
/// </summary>
public interface IPresentationData
{
    /// <summary>
    /// Gets the unique identifier of the presentation.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the title of the presentation.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the abstract of the presentation.
    /// </summary>
    string Abstract { get; }

    /// <summary>
    /// Gets the room where the presentation is held.
    /// </summary>
    string Room { get; }

    /// <summary>
    /// Gets the start date/time of the presentation.
    /// </summary>
    DateTime StartDateTime { get; }

    /// <summary>
    /// Gets the end date/time of the presentation.
    /// </summary>
    DateTime EndDateTime { get; }

    /// <summary>
    /// Gets the collection of speakers for the presentation.
    /// </summary>
    IReadOnlyCollection<IPresentationSpeaker> Speakers { get; }
}
