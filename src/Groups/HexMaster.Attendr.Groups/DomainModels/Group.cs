namespace HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// Aggregate root representing a group in the Attendr system.
/// Follows Domain-Driven Design principles with private constructor,
/// encapsulated collections, and behavior-focused methods.
/// </summary>
public sealed class Group
{
    /// <summary>
    /// Gets the unique identifier for the group.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the name of the group.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the settings for the group.
    /// </summary>
    public GroupSettings Settings { get; private set; }

    private readonly List<GroupMember> _members = new();
    private readonly List<GroupInvitation> _invitations = new();
    private readonly List<GroupJoinRequest> _joinRequests = new();

    /// <summary>
    /// Gets the collection of members in the group.
    /// </summary>
    public IReadOnlyCollection<GroupMember> Members => _members.AsReadOnly();

    /// <summary>
    /// Gets the collection of pending invitations for the group.
    /// </summary>
    public IReadOnlyCollection<GroupInvitation> Invitations => _invitations.AsReadOnly();

    /// <summary>
    /// Gets the collection of pending join requests for the group.
    /// </summary>
    public IReadOnlyCollection<GroupJoinRequest> JoinRequests => _joinRequests.AsReadOnly();

    private Group()
    {
        // For ORM/deserialization
        Id = Guid.Empty;
        Name = string.Empty;
        Settings = GroupSettings.CreateDefault();
    }

    private Group(Guid id, string name, Guid ownerId, string ownerName, GroupSettings? settings = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Group ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Group name cannot be empty.", nameof(name));
        }

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner ID cannot be empty.", nameof(ownerId));
        }

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            throw new ArgumentException("Owner name cannot be empty.", nameof(ownerName));
        }

        Id = id;
        Name = name;
        Settings = settings ?? GroupSettings.CreateDefault();

        // Ensure the group always has an owner
        _members.Add(new GroupMember(ownerId, ownerName, GroupRole.Owner));
    }

    /// <summary>
    /// Factory method to create a new group with a specified owner.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="ownerId">The ID of the group owner.</param>
    /// <param name="ownerName">The name of the group owner.</param>
    /// <param name="isPublic">Whether the group should be public.</param>
    /// <param name="isSearchable">Whether the group should be searchable.</param>
    /// <returns>A new instance of <see cref="Group"/>.</returns>
    public static Group Create(
        string name,
        Guid ownerId,
        string ownerName,
        bool isPublic = false,
        bool isSearchable = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        var id = Guid.NewGuid();
        var settings = GroupSettings.Create(isPublic, isSearchable);
        return new Group(id, name, ownerId, ownerName, settings);
    }

    /// <summary>
    /// Factory method to create a group from persisted data.
    /// </summary>
    /// <param name="id">The ID of the group.</param>
    /// <param name="name">The name of the group.</param>
    /// <param name="ownerId">The ID of the group owner.</param>
    /// <param name="ownerName">The name of the group owner.</param>
    /// <param name="settings">The group settings.</param>
    /// <returns>A new instance of <see cref="Group"/>.</returns>
    public static Group FromPersisted(
        Guid id,
        string name,
        Guid ownerId,
        string ownerName,
        GroupSettings? settings = null)
    {
        return new Group(id, name, ownerId, ownerName, settings);
    }

    /// <summary>
    /// Updates the name of the group.
    /// </summary>
    /// <param name="newName">The new name for the group.</param>
    /// <exception cref="ArgumentException">Thrown when the name is null or whitespace.</exception>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Group name cannot be empty.", nameof(newName));
        }

        Name = newName;
    }

    /// <summary>
    /// Updates the settings for the group.
    /// </summary>
    /// <param name="newSettings">The new settings for the group.</param>
    /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
    public void UpdateSettings(GroupSettings newSettings)
    {
        Settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
    }

    /// <summary>
    /// Adds a new member to the group.
    /// </summary>
    /// <param name="memberId">The ID of the member to add.</param>
    /// <param name="memberName">The name of the member.</param>
    /// <param name="role">The role of the member in the group.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add an owner or duplicate member.</exception>
    public void AddMember(Guid memberId, string memberName, GroupRole role)
    {
        if (role == GroupRole.Owner)
        {
            throw new InvalidOperationException("Cannot add another owner. Use TransferOwnership instead.");
        }

        if (_members.Any(m => m.Id == memberId))
        {
            throw new InvalidOperationException("Member already exists in the group.");
        }

        _members.Add(new GroupMember(memberId, memberName, role));
    }

    /// <summary>
    /// Removes a member from the group.
    /// </summary>
    /// <param name="memberId">The ID of the member to remove.</param>
    /// <exception cref="InvalidOperationException">Thrown when the member is not found or is the owner.</exception>
    public void RemoveMember(Guid memberId)
    {
        var member = _members.FirstOrDefault(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found in the group.");
        }

        if (member.Role == GroupRole.Owner)
        {
            throw new InvalidOperationException("Cannot remove the owner. Transfer ownership first.");
        }

        _members.Remove(member);
    }

    /// <summary>
    /// Updates the role of a member in the group.
    /// </summary>
    /// <param name="memberId">The ID of the member.</param>
    /// <param name="newRole">The new role for the member.</param>
    /// <exception cref="InvalidOperationException">Thrown when the member is not found or when attempting invalid owner changes.</exception>
    public void UpdateMemberRole(Guid memberId, GroupRole newRole)
    {
        var member = _members.FirstOrDefault(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found in the group.");
        }

        if (member.Role == GroupRole.Owner && newRole != GroupRole.Owner)
        {
            throw new InvalidOperationException("Cannot change owner's role. Use TransferOwnership instead.");
        }

        if (newRole == GroupRole.Owner)
        {
            throw new InvalidOperationException("Cannot promote to owner. Use TransferOwnership instead.");
        }

        member.UpdateRole(newRole);
    }

    /// <summary>
    /// Transfers group ownership to an existing member.
    /// </summary>
    /// <param name="newOwnerId">The ID of the new owner (must be an existing member).</param>
    /// <exception cref="InvalidOperationException">Thrown when no current owner exists or new owner is not a member.</exception>
    public void TransferOwnership(Guid newOwnerId)
    {
        var currentOwner = _members.FirstOrDefault(m => m.Role == GroupRole.Owner);
        var newOwner = _members.FirstOrDefault(m => m.Id == newOwnerId);

        if (currentOwner == null)
        {
            throw new InvalidOperationException("No current owner found.");
        }

        if (newOwner == null)
        {
            throw new InvalidOperationException("New owner must be an existing member of the group.");
        }

        currentOwner.UpdateRole(GroupRole.Manager);
        newOwner.UpdateRole(GroupRole.Owner);
    }

    /// <summary>
    /// Adds a pending invitation to the group.
    /// </summary>
    /// <param name="inviteeId">The ID of the user being invited.</param>
    /// <param name="inviteeName">The name of the user being invited.</param>
    /// <param name="expirationDate">The expiration date/time for the invitation.</param>
    /// <exception cref="InvalidOperationException">Thrown when user is already a member or has an active invitation.</exception>
    public void AddInvitation(Guid inviteeId, string inviteeName, DateTimeOffset expirationDate)
    {
        if (_members.Any(m => m.Id == inviteeId))
        {
            throw new InvalidOperationException("User is already a member of the group.");
        }

        if (_invitations.Any(i => i.Id == inviteeId && !i.IsExpired()))
        {
            throw new InvalidOperationException("An active invitation already exists for this user.");
        }

        var acceptanceCode = GroupInvitation.GenerateAcceptanceCode();
        _invitations.Add(new GroupInvitation(inviteeId, inviteeName, acceptanceCode, expirationDate));
    }

    /// <summary>
    /// Removes a pending invitation from the group.
    /// </summary>
    /// <param name="inviteeId">The ID of the user whose invitation should be removed.</param>
    /// <exception cref="InvalidOperationException">Thrown when the invitation is not found.</exception>
    public void RemoveInvitation(Guid inviteeId)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Id == inviteeId);

        if (invitation == null)
        {
            throw new InvalidOperationException("Invitation not found.");
        }

        _invitations.Remove(invitation);
    }

    /// <summary>
    /// Accepts a pending group invitation and adds the user as a member.
    /// </summary>
    /// <param name="inviteeId">The ID of the user accepting the invitation.</param>
    /// <param name="acceptanceCode">The acceptance code for validating the invitation.</param>
    /// <exception cref="InvalidOperationException">Thrown when invitation is not found, expired, or code is invalid.</exception>
    public void AcceptInvitation(Guid inviteeId, string acceptanceCode)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Id == inviteeId);

        if (invitation == null)
        {
            throw new InvalidOperationException("Invitation not found.");
        }

        if (invitation.IsExpired())
        {
            _invitations.Remove(invitation);
            throw new InvalidOperationException("Invitation has expired.");
        }

        if (!invitation.AcceptanceCode.Equals(acceptanceCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid acceptance code.");
        }

        AddMember(invitation.Id, invitation.Name, GroupRole.Member);
        _invitations.Remove(invitation);
    }

    /// <summary>
    /// Removes all expired invitations from the group.
    /// </summary>
    public void CleanupExpiredInvitations()
    {
        _invitations.RemoveAll(i => i.IsExpired());
    }

    /// <summary>
    /// Adds a join request to the group.
    /// </summary>
    /// <param name="profileId">The ID of the profile requesting to join.</param>
    /// <param name="profileName">The name of the profile requesting to join.</param>
    /// <exception cref="InvalidOperationException">Thrown when user is already a member or has a pending request.</exception>
    public void AddJoinRequest(Guid profileId, string profileName)
    {
        if (_members.Any(m => m.Id == profileId))
        {
            throw new InvalidOperationException("User is already a member of the group.");
        }

        if (_joinRequests.Any(jr => jr.Id == profileId))
        {
            throw new InvalidOperationException("User already has a pending join request.");
        }

        _joinRequests.Add(new GroupJoinRequest(profileId, profileName, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Approves a join request and adds the user as a member.
    /// </summary>
    /// <param name="profileId">The ID of the profile whose request is being approved.</param>
    /// <exception cref="InvalidOperationException">Thrown when the join request is not found.</exception>
    public void ApproveJoinRequest(Guid profileId)
    {
        var joinRequest = _joinRequests.FirstOrDefault(jr => jr.Id == profileId);

        if (joinRequest == null)
        {
            throw new InvalidOperationException("Join request not found.");
        }

        AddMember(joinRequest.Id, joinRequest.Name, GroupRole.Member);
        _joinRequests.Remove(joinRequest);
    }

    /// <summary>
    /// Declines a join request and removes it from the group.
    /// </summary>
    /// <param name="profileId">The ID of the profile whose request is being declined.</param>
    /// <exception cref="InvalidOperationException">Thrown when the join request is not found.</exception>
    public void DeclineJoinRequest(Guid profileId)
    {
        var joinRequest = _joinRequests.FirstOrDefault(jr => jr.Id == profileId);

        if (joinRequest == null)
        {
            throw new InvalidOperationException("Join request not found.");
        }

        _joinRequests.Remove(joinRequest);
    }

    /// <summary>
    /// Gets the current owner of the group.
    /// </summary>
    /// <returns>The group member who is the owner.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no owner exists.</exception>
    public GroupMember GetOwner()
    {
        return _members.First(m => m.Role == GroupRole.Owner);
    }
}
