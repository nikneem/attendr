namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing a group invitation.
/// </summary>
public interface IGroupInvitation
{
    /// <summary>
    /// Gets the unique identifier of the invited user.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the invited user.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the acceptance code for the invitation.
    /// </summary>
    string AcceptanceCode { get; }

    /// <summary>
    /// Gets the expiration date/time for the invitation.
    /// </summary>
    DateTimeOffset ExpirationDate { get; }

    /// <summary>
    /// Determines whether the invitation is expired.
    /// </summary>
    /// <returns>True if expired; otherwise, false.</returns>
    bool IsExpired();
}
