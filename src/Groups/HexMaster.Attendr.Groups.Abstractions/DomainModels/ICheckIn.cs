namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing a group check-in to a presentation.
/// </summary>
public interface ICheckIn
{
    /// <summary>
    /// Gets the unique identifier for the check-in.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the group identifier.
    /// </summary>
    Guid GroupId { get; }

    /// <summary>
    /// Gets the conference identifier.
    /// </summary>
    Guid ConferenceId { get; }

    /// <summary>
    /// Gets the presentation identifier.
    /// </summary>
    Guid PresentationId { get; }

    /// <summary>
    /// Gets the presentation data.
    /// </summary>
    IPresentationData PresentationData { get; }

    /// <summary>
    /// Gets the expiration date/time for the check-in.
    /// </summary>
    DateTimeOffset Expiration { get; }

    /// <summary>
    /// Gets the collection of checked-in members.
    /// </summary>
    IReadOnlyCollection<ICheckedInMember> Members { get; }

    /// <summary>
    /// Adds a member to the check-in.
    /// </summary>
    /// <param name="member">The member to add.</param>
    void AddMember(ICheckedInMember member);

    /// <summary>
    /// Removes a member from the check-in.
    /// </summary>
    /// <param name="memberId">The ID of the member to remove.</param>
    void RemoveMember(Guid memberId);

    /// <summary>
    /// Determines whether the check-in is expired.
    /// </summary>
    /// <param name="now">The current date/time to compare against.</param>
    /// <returns>True if expired; otherwise, false.</returns>
    bool IsExpired(DateTimeOffset now);
}
