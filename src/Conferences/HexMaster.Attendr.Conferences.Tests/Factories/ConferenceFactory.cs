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
        SynchronizationSource? synchronizationSource = null,
        Guid? createdByProfileId = null)
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
            synchronizationSource,
            createdByProfileId
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
        SynchronizationSource? synchronizationSource = null,
        Guid? createdByProfileId = null)
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
            false,
            synchronizationSource,
            createdByProfileId
        );
    }

    /// <summary>
    /// Creates a Room for testing.
    /// </summary>
    public static Room CreateRoom(string? name = null, int? capacity = null)
    {
        return Room.Create(
            name ?? Faker.Commerce.Department() + " Room",
            capacity ?? Faker.Random.Int(50, 500)
        );
    }

    /// <summary>
    /// Creates a Speaker for testing.
    /// </summary>
    public static Speaker CreateSpeaker(string? name = null, string? company = null)
    {
        return Speaker.Create(
            name ?? Faker.Name.FullName(),
            company ?? Faker.Company.CompanyName()
        );
    }

    /// <summary>
    /// Creates a Presentation for testing.
    /// </summary>
    public static Presentation CreatePresentation(
        string? title = null,
        string? abstractText = null,
        DateTimeOffset? startDateTime = null,
        DateTimeOffset? endDateTime = null,
        Room? room = null,
        IEnumerable<Speaker>? speakers = null)
    {
        var start = startDateTime ?? new DateTimeOffset(DateTime.SpecifyKind(Faker.Date.Future(), DateTimeKind.Utc), TimeSpan.Zero);
        var end = endDateTime ?? start.AddHours(1);

        return Presentation.Create(
            title ?? Faker.Lorem.Sentence(),
            abstractText ?? Faker.Lorem.Paragraph(),
            start,
            end,
            room ?? Room.Create(Faker.Lorem.Word(), Faker.Random.Int(50, 500)),
            speakers ?? new[] { Speaker.Create(Faker.Name.FullName()) }
        );
    }
}
