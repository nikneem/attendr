namespace HexMaster.Attendr.Groups.DomainModels;

public sealed class GroupMember
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public GroupRole Role { get; private set; }

    public GroupMember(Guid id, string name, GroupRole role)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Member ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Member name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
        Role = role;
    }

    public void UpdateRole(GroupRole newRole)
    {
        Role = newRole;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Member name cannot be empty.", nameof(newName));
        }

        Name = newName;
    }
}
