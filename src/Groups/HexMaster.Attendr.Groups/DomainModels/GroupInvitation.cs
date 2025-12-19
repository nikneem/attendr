namespace HexMaster.Attendr.Groups.DomainModels;

public sealed class GroupInvitation
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string AcceptanceCode { get; private set; }
    public DateTimeOffset ExpirationDate { get; private set; }

    public GroupInvitation(Guid id, string name, string acceptanceCode, DateTimeOffset expirationDate)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invitation ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Invitee name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(acceptanceCode))
        {
            throw new ArgumentException("Acceptance code cannot be empty.", nameof(acceptanceCode));
        }

        if (acceptanceCode.Length != 8)
        {
            throw new ArgumentException("Acceptance code must be exactly 8 characters.", nameof(acceptanceCode));
        }

        if (expirationDate <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("Expiration date must be in the future.", nameof(expirationDate));
        }

        Id = id;
        Name = name;
        AcceptanceCode = acceptanceCode.ToUpperInvariant();
        ExpirationDate = expirationDate;
    }

    public bool IsExpired()
    {
        return DateTimeOffset.UtcNow >= ExpirationDate;
    }

    public static string GenerateAcceptanceCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }
}
