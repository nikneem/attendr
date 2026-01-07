using Bogus;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.Factories;

public static class GroupFactory
{
    private static readonly Faker Faker = new();

    public static Group CreateGroup(
        string? name = null,
        Guid? ownerId = null,
        string? ownerName = null,
        bool isPublic = false,
        bool isSearchable = true)
    {
        return Group.Create(
            name ?? Faker.Company.CompanyName(),
            ownerId ?? Guid.NewGuid(),
            ownerName ?? Faker.Person.FullName,
            isPublic,
            isSearchable);
    }

    public static Group CreatePersistedGroup(
        Guid? id = null,
        string? name = null,
        Guid? ownerId = null,
        string? ownerName = null,
        GroupSettings? settings = null)
    {
        return Group.FromPersisted(
            id ?? Guid.NewGuid(),
            name ?? Faker.Company.CompanyName(),
            ownerId ?? Guid.NewGuid(),
            ownerName ?? Faker.Person.FullName,
            settings);
    }

    public static GroupMember CreateMember(
        Guid? id = null,
        string? name = null,
        GroupRole role = GroupRole.Member)
    {
        return new GroupMember(
            id ?? Guid.NewGuid(),
            name ?? Faker.Person.FullName,
            role);
    }
}
