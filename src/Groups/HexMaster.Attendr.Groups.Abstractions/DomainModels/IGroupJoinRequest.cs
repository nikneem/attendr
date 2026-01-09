namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing a join request for a group.
/// </summary>
public interface IGroupJoinRequest
{
    /// <summary>
    /// Gets the unique identifier of the user requesting to join.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the user requesting to join.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the date/time when the request was created.
    /// </summary>
    DateTimeOffset RequestDate { get; }
}
