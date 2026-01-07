using Bogus;
using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.Factories;

/// <summary>
/// Factory for creating Conference test data using Bogus.
/// </summary>
public static class ConferenceFactory
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Creates a new Conference domain model for testing.
    /// </summary>
    public static Conference CreateConference(
        string? title = null,
        string? city = null,
        string? country = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        string? imageUrl = null,
        SynchronizationSource? synchronizationSource = null)
    {
        var start = startDate ?? DateOnly.FromDateTime(Faker.Date.Future());
        var end = endDate ?? start.AddDays(Faker.Random.Int(1, 5));

        return Conference.Create(
            title ?? Faker.Company.CompanyName() + " Conference",
            city ?? Faker.Address.City(),
            country ?? Faker.Address.Country(),
            start,
            end,
            imageUrl ?? (Faker.Random.Bool() ? Faker.Internet.Url() : null),
            synchronizationSource
        );
    }

    /// <summary>
    /// Creates a Conference from persisted data for testing.
    /// </summary>
    public static Conference CreatePersistedConference(
        Guid? id = null,
        string? title = null,
        string? city = null,
        string? country = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        string? imageUrl = null,
        SynchronizationSource? synchronizationSource = null)
    {
        var start = startDate ?? DateOnly.FromDateTime(Faker.Date.Future());
        var end = endDate ?? start.AddDays(Faker.Random.Int(1, 5));

        return Conference.FromPersisted(
            id ?? Guid.NewGuid(),
            title ?? Faker.Company.CompanyName() + " Conference",
            city ?? Faker.Address.City(),
            country ?? Faker.Address.Country(),
            start,
            end,
            imageUrl ?? (Faker.Random.Bool() ? Faker.Internet.Url() : null),
            synchronizationSource
        );
    }

    /// <summary>
    /// Creates a Room for testing.
    /// </summary>
    public static Room CreateRoom(Guid? id = null, string? name = null)
    {
        return Room.Create(
            id ?? Guid.NewGuid(),
            name ?? Faker.Commerce.Department() + " Room"
        );
    }

    /// <summary>
    /// Creates a Speaker for testing.
    /// </summary>
    public static Speaker CreateSpeaker(Guid? id = null, string? name = null)
    {
        return Speaker.Create(
            id ?? Guid.NewGuid(),
            name ?? Faker.Name.FullName()
        );
    }

    /// <summary>
    /// Creates a Presentation for testing.
    /// </summary>
    public static Presentation CreatePresentation(
        Guid? id = null,
        Guid? roomId = null,
        string? title = null,
        DateTime? startDateTime = null,
        DateTime? endDateTime = null,
        IEnumerable<Guid>? speakerIds = null)
    {
        var start = startDateTime ?? Faker.Date.Future();
        var end = endDateTime ?? start.AddHours(1);

        return Presentation.Create(
            id ?? Guid.NewGuid(),
            roomId ?? Guid.NewGuid(),
            title ?? Faker.Lorem.Sentence(),
            start,
            end,
            speakerIds ?? new[] { Guid.NewGuid() }
        );
    }
}
