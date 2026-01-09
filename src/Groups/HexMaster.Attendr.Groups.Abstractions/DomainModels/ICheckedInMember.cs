namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing a checked-in member.
/// </summary>
public interface ICheckedInMember
{
    /// <summary>
    /// Gets the unique identifier of the member.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the optional profile picture URL of the member.
    /// </summary>
    string? ProfilePictureUrl { get; }
}
