using Bogus;
using HexMaster.Attendr.Profiles.Data.MongoDb.Mappers;
using HexMaster.Attendr.Profiles.Data.MongoDb.Models;
using HexMaster.Attendr.Profiles.Tests.Factories;

namespace HexMaster.Attendr.Profiles.Tests.DataAccess;

public class ProfileMapperTests
{
    private readonly Faker _faker;

    public ProfileMapperTests()
    {
        _faker = new Faker();
    }

    [Fact]
    public void ToDocument_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var profile = ProfileFactory.CreatePersistedProfile(
            id: _faker.Random.Guid().ToString(),
            subjectId: _faker.Random.Guid().ToString(),
            displayName: _faker.Person.FullName,
            firstName: _faker.Person.FirstName,
            lastName: _faker.Person.LastName,
            email: _faker.Person.Email,
            employee: _faker.Random.AlphaNumeric(10),
            tagLine: _faker.Lorem.Sentence(),
            isEnabled: true,
            isSearchable: true
        );

        // Act
        var document = ProfileMapper.ToDocument(profile);

        // Assert
        Assert.Equal(profile.Id, document.Id);
        Assert.Equal(profile.SubjectId, document.SubjectId);
        Assert.Equal(profile.DisplayName, document.DisplayName);
        Assert.Equal(profile.FirstName, document.FirstName);
        Assert.Equal(profile.LastName, document.LastName);
        Assert.Equal(profile.Email, document.Email);
        Assert.Equal(profile.Employee, document.Employee);
        Assert.Equal(profile.TagLine, document.TagLine);
        Assert.Equal(profile.IsSearchable, document.IsSearchable);
        Assert.Equal(profile.Enabled, document.Enabled);
    }

    [Fact]
    public void ToDocument_ShouldMapNullableFieldsCorrectly()
    {
        // Arrange
        var profile = ProfileFactory.CreatePersistedProfile(
            employee: null,
            tagLine: null
        );

        // Act
        var document = ProfileMapper.ToDocument(profile);

        // Assert
        Assert.Null(document.Employee);
        Assert.Null(document.TagLine);
    }

    [Fact]
    public void ToDomain_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var document = new ProfileDocument
        {
            Id = _faker.Random.Guid().ToString(),
            SubjectId = _faker.Random.Guid().ToString(),
            DisplayName = _faker.Person.FullName,
            FirstName = _faker.Person.FirstName,
            LastName = _faker.Person.LastName,
            Email = _faker.Person.Email,
            Employee = _faker.Random.AlphaNumeric(10),
            TagLine = _faker.Lorem.Sentence(),
            IsSearchable = true,
            Enabled = true
        };

        // Act
        var profile = ProfileMapper.ToDomain(document);

        // Assert
        Assert.Equal(document.Id, profile.Id);
        Assert.Equal(document.SubjectId, profile.SubjectId);
        Assert.Equal(document.DisplayName, profile.DisplayName);
        Assert.Equal(document.FirstName, profile.FirstName);
        Assert.Equal(document.LastName, profile.LastName);
        Assert.Equal(document.Email.ToLowerInvariant(), profile.Email); // Profile converts email to lowercase
        Assert.Equal(document.Employee, profile.Employee);
        Assert.Equal(document.TagLine, profile.TagLine);
        Assert.Equal(document.IsSearchable, profile.IsSearchable);
        Assert.Equal(document.Enabled, profile.Enabled);
    }

    [Fact]
    public void ToDomain_ShouldConvertNullFirstNameToEmptyString()
    {
        // Arrange
        var document = new ProfileDocument
        {
            Id = _faker.Random.Guid().ToString(),
            SubjectId = _faker.Random.Guid().ToString(),
            DisplayName = _faker.Person.FullName,
            FirstName = null,
            LastName = _faker.Person.LastName,
            Email = _faker.Person.Email,
            IsSearchable = false,
            Enabled = true
        };

        // Act
        var profile = ProfileMapper.ToDomain(document);

        // Assert
        Assert.Null(profile.FirstName);
    }

    [Fact]
    public void ToDomain_ShouldConvertNullLastNameToEmptyString()
    {
        // Arrange
        var document = new ProfileDocument
        {
            Id = _faker.Random.Guid().ToString(),
            SubjectId = _faker.Random.Guid().ToString(),
            DisplayName = _faker.Person.FullName,
            FirstName = _faker.Person.FirstName,
            LastName = null,
            Email = _faker.Person.Email,
            IsSearchable = false,
            Enabled = true
        };

        // Act
        var profile = ProfileMapper.ToDomain(document);

        // Assert
        Assert.Null(profile.LastName);
    }

    [Fact]
    public void ToDomain_ShouldHandleNullableFieldsCorrectly()
    {
        // Arrange
        var document = new ProfileDocument
        {
            Id = _faker.Random.Guid().ToString(),
            SubjectId = _faker.Random.Guid().ToString(),
            DisplayName = _faker.Person.FullName,
            FirstName = _faker.Person.FirstName,
            LastName = _faker.Person.LastName,
            Email = _faker.Person.Email,
            Employee = null,
            TagLine = null,
            IsSearchable = false,
            Enabled = true
        };

        // Act
        var profile = ProfileMapper.ToDomain(document);

        // Assert
        Assert.Null(profile.Employee);
        Assert.Null(profile.TagLine);
    }

    [Fact]
    public void RoundTrip_ShouldPreserveAllData()
    {
        // Arrange
        var originalProfile = ProfileFactory.CreatePersistedProfile(
            id: _faker.Random.Guid().ToString(),
            subjectId: _faker.Random.Guid().ToString(),
            displayName: _faker.Person.FullName,
            firstName: _faker.Person.FirstName,
            lastName: _faker.Person.LastName,
            email: _faker.Person.Email,
            employee: _faker.Random.AlphaNumeric(10),
            tagLine: _faker.Lorem.Sentence(),
            isEnabled: true,
            isSearchable: true
        );

        // Act
        var document = ProfileMapper.ToDocument(originalProfile);
        var roundTrippedProfile = ProfileMapper.ToDomain(document);

        // Assert
        Assert.Equal(originalProfile.Id, roundTrippedProfile.Id);
        Assert.Equal(originalProfile.SubjectId, roundTrippedProfile.SubjectId);
        Assert.Equal(originalProfile.DisplayName, roundTrippedProfile.DisplayName);
        Assert.Equal(originalProfile.FirstName, roundTrippedProfile.FirstName);
        Assert.Equal(originalProfile.LastName, roundTrippedProfile.LastName);
        Assert.Equal(originalProfile.Email, roundTrippedProfile.Email);
        Assert.Equal(originalProfile.Employee, roundTrippedProfile.Employee);
        Assert.Equal(originalProfile.TagLine, roundTrippedProfile.TagLine);
        Assert.Equal(originalProfile.IsSearchable, roundTrippedProfile.IsSearchable);
        Assert.Equal(originalProfile.Enabled, roundTrippedProfile.Enabled);
    }
}
