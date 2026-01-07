using Bogus;
using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Tests.Factories;

/// <summary>
/// Factory for creating Profile test data using Bogus.
/// </summary>
public static class ProfileFactory
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Creates a new Profile domain model for testing.
    /// </summary>
    public static Profile CreateProfile(
        string? subjectId = null,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null)
    {
        return Profile.Create(
            subjectId ?? Faker.Random.Guid().ToString(),
            displayName ?? Faker.Person.FullName,
            firstName ?? Faker.Person.FirstName,
            lastName ?? Faker.Person.LastName,
            email ?? Faker.Person.Email
        );
    }

    /// <summary>
    /// Creates a Profile from persisted data for testing.
    /// </summary>
    public static Profile CreatePersistedProfile(
        string? id = null,
        string? subjectId = null,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? employee = null,
        string? tagLine = null,
        bool isEnabled = true,
        bool isSearchable = false)
    {
        return Profile.FromPersisted(
            id ?? Faker.Random.Guid().ToString(),
            subjectId ?? Faker.Random.Guid().ToString(),
            displayName ?? Faker.Person.FullName,
            firstName ?? Faker.Person.FirstName,
            lastName ?? Faker.Person.LastName,
            email ?? Faker.Person.Email,
            employee,
            tagLine,
            isEnabled,
            isSearchable
        );
    }
}
