namespace HexMaster.Attendr.Groups.DomainModels;

public sealed class Group
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public GroupSettings Settings { get; private set; }
    private readonly List<GroupMember> _members = new();
    private readonly List<GroupInvitation> _invitations = new();

    public IReadOnlyCollection<GroupMember> Members => _members.AsReadOnly();
    public IReadOnlyCollection<GroupInvitation> Invitations => _invitations.AsReadOnly();

    private Group()
    {
        // For ORM/deserialization
        Id = Guid.Empty;
        Name = string.Empty;
        Settings = new GroupSettings();
    }

    public Group(Guid id, string name, Guid ownerId, string ownerName, GroupSettings? settings = null)
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
        Settings = settings ?? new GroupSettings();

        // Ensure the group always has an owner
        _members.Add(new GroupMember(ownerId, ownerName, GroupRole.Owner));
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Group name cannot be empty.", nameof(newName));
        }

        Name = newName;
    }

    public void UpdateSettings(GroupSettings newSettings)
    {
        Settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
    }

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

    public void RemoveInvitation(Guid inviteeId)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Id == inviteeId);

        if (invitation == null)
        {
            throw new InvalidOperationException("Invitation not found.");
        }

        _invitations.Remove(invitation);
    }

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

    public void CleanupExpiredInvitations()
    {
        _invitations.RemoveAll(i => i.IsExpired());
    }

    public GroupMember GetOwner()
    {
        return _members.First(m => m.Role == GroupRole.Owner);
    }
}
