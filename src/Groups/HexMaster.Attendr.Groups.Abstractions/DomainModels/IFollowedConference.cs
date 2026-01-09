namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing a conference followed by a group.
/// </summary>
public interface IFollowedConference
{
    /// <summary>
    /// Gets the unique identifier of the conference.
    /// </summary>
    Guid ConferenceId { get; }

    /// <summary>
    /// Gets the name of the conference.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the city where the conference is held.
    /// </summary>
    string City { get; }

    /// <summary>
    /// Gets the country where the conference is held.
    /// </summary>
    string Country { get; }

    /// <summary>
    /// Gets the optional image URL for the conference.
    /// </summary>
    string? ImageUrl { get; }

    /// <summary>
    /// Gets the number of speakers at the conference.
    /// </summary>
    int SpeakersCount { get; }

    /// <summary>
    /// Gets the number of sessions at the conference.
    /// </summary>
    int SessionsCount { get; }

    /// <summary>
    /// Gets the start date of the conference.
    /// </summary>
    DateOnly StartDate { get; }

    /// <summary>
    /// Gets the end date of the conference.
    /// </summary>
    DateOnly EndDate { get; }

    /// <summary>
    /// Determines whether the conference is current or in the future.
    /// </summary>
    /// <returns>True if current or future; otherwise, false.</returns>
    bool IsCurrentOrFuture();
}
