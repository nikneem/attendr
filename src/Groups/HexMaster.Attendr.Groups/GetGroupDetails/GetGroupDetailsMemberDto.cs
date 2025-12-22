using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.GetGroupDetails;

/// <summary>
/// DTO representing a member in a group for details response.
/// </summary>
/// <param name="Id">The unique identifier of the member.</param>
/// <param name="Name">The name of the member.</param>
/// <param name="Role">The role of the member in the group.</param>
public sealed record GetGroupDetailsMemberDto(
    Guid Id,
    string Name,
    GroupRole Role);
