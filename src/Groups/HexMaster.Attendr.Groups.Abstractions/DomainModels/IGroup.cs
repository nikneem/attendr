namespace HexMaster.Attendr.Groups.Abstractions.DomainModels;

/// <summary>
/// Interface representing a group aggregate root.
/// </summary>
public interface IGroup
{
    /// <summary>
    /// Gets the unique identifier for the group.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the settings for the group.
    /// </summary>
    IGroupSettings Settings { get; }

    /// <summary>
    /// Gets the collection of members in the group.
    /// </summary>
    IReadOnlyCollection<IGroupMember> Members { get; }

    /// <summary>
    /// Gets the collection of pending invitations for the group.
    /// </summary>
    IReadOnlyCollection<IGroupInvitation> Invitations { get; }

    /// <summary>
    /// Gets the collection of pending join requests for the group.
    /// </summary>
    IReadOnlyCollection<IGroupJoinRequest> JoinRequests { get; }

    /// <summary>
    /// Gets the collection of conferences followed by the group.
    /// </summary>
    IReadOnlyCollection<IFollowedConference> FollowedConferences { get; }

    /// <summary>
    /// Gets the collection of group activities.
    /// </summary>
    IReadOnlyCollection<IGroupActivity> Activities { get; }

    /// <summary>
    /// Updates the name of the group.
    /// </summary>
    /// <param name="newName">The new name for the group.</param>
    void UpdateName(string newName);

    /// <summary>
    /// Updates the settings for the group.
    /// </summary>
    /// <param name="newSettings">The new settings for the group.</param>
    void UpdateSettings(IGroupSettings newSettings);

    /// <summary>
    /// Adds a new member to the group.
    /// </summary>
    /// <param name="memberId">The ID of the member to add.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="role">The role of the member in the group.</param>
    void AddMember(Guid memberId, string memberName, GroupRole role);

    /// <summary>
    /// Removes a member from the group.
    /// </summary>
    /// <param name="memberId">The ID of the member to remove.</param>
    void RemoveMember(Guid memberId);

    /// <summary>
    /// Updates the role of a member in the group.
    /// </summary>
    /// <param name="memberId">The ID of the member.</param>
    /// <param name="newRole">The new role for the member.</param>
    void UpdateMemberRole(Guid memberId, GroupRole newRole);

    /// <summary>
    /// Transfers group ownership to an existing member.
    /// </summary>
    /// <param name="newOwnerId">The ID of the new owner.</param>
    void TransferOwnership(Guid newOwnerId);

    /// <summary>
    /// Adds a pending invitation to the group.
    /// </summary>
    /// <param name="inviteeId">The ID of the user being invited.</param>
    /// <param name="inviteeName">The name of the user being invited.</param>
    /// <param name="expirationDate">The expiration date/time for the invitation.</param>
    void AddInvitation(Guid inviteeId, string inviteeName, DateTimeOffset expirationDate);

    /// <summary>
    /// Removes a pending invitation from the group.
    /// </summary>
    /// <param name="inviteeId">The ID of the user whose invitation should be removed.</param>
    void RemoveInvitation(Guid inviteeId);

    /// <summary>
    /// Accepts a pending group invitation and adds the user as a member.
    /// </summary>
    /// <param name="inviteeId">The ID of the user accepting the invitation.</param>
    /// <param name="acceptanceCode">The acceptance code for validating the invitation.</param>
    void AcceptInvitation(Guid inviteeId, string acceptanceCode);

    /// <summary>
    /// Removes all expired invitations from the group.
    /// </summary>
    void CleanupExpiredInvitations();

    /// <summary>
    /// Adds a join request to the group.
    /// </summary>
    /// <param name="profileId">The ID of the profile requesting to join.</param>
    /// <param name="profileName">The name of the profile requesting to join.</param>
    void AddJoinRequest(Guid profileId, string profileName);

    /// <summary>
    /// Approves a join request and adds the user as a member.
    /// </summary>
    /// <param name="profileId">The ID of the profile whose request is being approved.</param>
    void ApproveJoinRequest(Guid profileId);

    /// <summary>
    /// Declines a join request and removes it from the group.
    /// </summary>
    /// <param name="profileId">The ID of the profile whose request is being declined.</param>
    void DeclineJoinRequest(Guid profileId);

    /// <summary>
    /// Gets the current owner of the group.
    /// </summary>
    /// <returns>The group member who is the owner.</returns>
    IGroupMember GetOwner();

    /// <summary>
    /// Adds a conference to the group's followed conferences list.
    /// </summary>
    /// <param name="conferenceId">The ID of the conference to follow.</param>
    /// <param name="name">The name of the conference.</param>
    /// <param name="city">The city where the conference is held.</param>
    /// <param name="country">The country where the conference is held.</param>
    /// <param name="imageUrl">Optional visual for the conference.</param>
    /// <param name="speakersCount">Total speakers count.</param>
    /// <param name="sessionsCount">Total sessions/presentations count.</param>
    /// <param name="startDate">The start date of the conference.</param>
    /// <param name="endDate">The end date of the conference.</param>
    void FollowConference(
        Guid conferenceId,
        string name,
        string city,
        string country,
        string? imageUrl,
        int speakersCount,
        int sessionsCount,
        DateOnly startDate,
        DateOnly endDate);

    /// <summary>
    /// Removes a conference from the group's followed conferences list.
    /// </summary>
    /// <param name="conferenceId">The ID of the conference to unfollow.</param>
    void UnfollowConference(Guid conferenceId);

    /// <summary>
    /// Gets all current and future conferences followed by the group.
    /// </summary>
    /// <returns>Collection of current and future followed conferences.</returns>
    IEnumerable<IFollowedConference> GetCurrentAndFutureFollowedConferences();

    /// <summary>
    /// Adds an activity to the group's activity log.
    /// </summary>
    /// <param name="profileId">The unique identifier of the profile that triggered this activity.</param>
    /// <param name="description">The description of the activity.</param>
    /// <param name="activityType">The type of the activity.</param>
    void AddActivity(Guid profileId, string description, GroupActivityType activityType);
}
