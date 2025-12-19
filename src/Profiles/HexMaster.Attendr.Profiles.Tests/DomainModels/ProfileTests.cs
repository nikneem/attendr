using Bogus;
using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Tests.DomainModels;

public class ProfileTests
{
    private readonly Faker _faker;

    public ProfileTests()
    {
        _faker = new Faker();
    }

    [Fact]
    public void Constructor_ShouldCreateProfile_WithValidParameters()
    {
        // Arrange
        var id = _faker.Random.Guid().ToString();
        var subjectId = _faker.Random.Guid().ToString();
        var displayName = _faker.Person.FullName;
        var firstName = _faker.Person.FirstName;
        var lastName = _faker.Person.LastName;
        var email = _faker.Person.Email;
        var employee = _faker.Random.AlphaNumeric(10);
        var tagLine = _faker.Lorem.Sentence();

        // Act
        var profile = Profile.FromPersisted(id, subjectId, displayName, firstName, lastName, email, employee, tagLine, true, false);

        // Assert
        Assert.Equal(id, profile.Id);
        Assert.Equal(subjectId, profile.SubjectId);
        Assert.Equal(displayName, profile.DisplayName);
        Assert.Equal(firstName, profile.FirstName);
        Assert.Equal(lastName, profile.LastName);
        Assert.Equal(email.ToLowerInvariant(), profile.Email);
        Assert.Equal(employee, profile.Employee);
        Assert.Equal(tagLine, profile.TagLine);
        Assert.True(profile.Enabled);
        Assert.False(profile.IsSearchable);
    }

    [Fact]
    public void Constructor_ShouldCreateProfile_WithNullableFields()
    {
        // Arrange
        var id = _faker.Random.Guid().ToString();
        var subjectId = _faker.Random.Guid().ToString();
        var displayName = _faker.Person.FullName;
        var firstName = _faker.Person.FirstName;
        var lastName = _faker.Person.LastName;
        var email = _faker.Person.Email;

        // Act
        var profile = Profile.FromPersisted(id, subjectId, displayName, firstName, lastName, email, null, null, true, false);

        // Assert
        Assert.Equal(id, profile.Id);
        Assert.Equal(firstName, profile.FirstName);
        Assert.Equal(lastName, profile.LastName);
        Assert.Null(profile.Employee);
        Assert.Null(profile.TagLine);
    }





    [Fact]
    public void SetFirstName_ShouldUpdateFirstName_WithValidValue()
    {
        // Arrange
        var profile = CreateValidProfile();
        var newFirstName = _faker.Person.FirstName;
        var oldModifiedOn = profile.ModifiedOn;

        // Act
        profile.SetFirstName(newFirstName);

        // Assert
        Assert.Equal(newFirstName, profile.FirstName);
        Assert.NotEqual(oldModifiedOn, profile.ModifiedOn);
    }

    [Fact]
    public void SetFirstName_ShouldThrowArgumentException_WhenValueIsEmpty()
    {
        // Arrange
        var profile = CreateValidProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetFirstName(string.Empty));
    }

    [Fact]
    public void SetLastName_ShouldUpdateLastName_WithValidValue()
    {
        // Arrange
        var profile = CreateValidProfile();
        var newLastName = _faker.Person.LastName;
        var oldModifiedOn = profile.ModifiedOn;

        // Act
        profile.SetLastName(newLastName);

        // Assert
        Assert.Equal(newLastName, profile.LastName);
        Assert.NotEqual(oldModifiedOn, profile.ModifiedOn);
    }

    [Fact]
    public void SetEmail_ShouldUpdateEmailInLowercase_WithValidValue()
    {
        // Arrange
        var profile = CreateValidProfile();
        var newEmail = "Test@Example.COM";
        var oldModifiedOn = profile.ModifiedOn;

        // Act
        profile.SetEmail(newEmail);

        // Assert
        Assert.Equal("test@example.com", profile.Email);
        Assert.NotEqual(oldModifiedOn, profile.ModifiedOn);
    }

    [Fact]
    public void SetEmail_ShouldThrowArgumentException_WhenEmailIsMissingAtSymbol()
    {
        // Arrange
        var profile = CreateValidProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetEmail("invalidemail.com"));
    }

    [Fact]
    public void SetDisplayName_ShouldUpdateDisplayName_WithValidValue()
    {
        // Arrange
        var profile = CreateValidProfile();
        var newDisplayName = _faker.Person.FullName;
        var oldModifiedOn = profile.ModifiedOn;

        // Act
        profile.SetDisplayName(newDisplayName);

        // Assert
        Assert.Equal(newDisplayName, profile.DisplayName);
        Assert.NotEqual(oldModifiedOn, profile.ModifiedOn);
    }

    [Fact]
    public void SetEmployee_ShouldUpdateEmployee_WithValidValue()
    {
        // Arrange
        var profile = CreateValidProfile();
        var employeeId = _faker.Random.AlphaNumeric(10);
        var oldModifiedOn = profile.ModifiedOn;

        // Act
        profile.SetEmployee(employeeId);

        // Assert
        Assert.Equal(employeeId, profile.Employee);
        Assert.NotEqual(oldModifiedOn, profile.ModifiedOn);
    }

    [Fact]
    public void SetTagLine_ShouldUpdateTagLine_WithValidValue()
    {
        // Arrange
        var profile = CreateValidProfile();
        var tagLine = _faker.Lorem.Sentence();
        var oldModifiedOn = profile.ModifiedOn;

        // Act
        profile.SetTagLine(tagLine);

        // Assert
        Assert.Equal(tagLine, profile.TagLine);
        Assert.NotEqual(oldModifiedOn, profile.ModifiedOn);
    }

    [Fact]
    public void SetTagLine_ShouldSetEmptyString_WhenValueIsNull()
    {
        // Arrange
        var profile = CreateValidProfile();

        // Act
        profile.SetTagLine(null);

        // Assert
        Assert.Equal(string.Empty, profile.TagLine);
    }

    [Fact]
    public void SetIsSearchable_ShouldUpdateIsSearchable()
    {
        // Arrange
        var profile = CreateValidProfile();
        var oldModifiedOn = profile.ModifiedOn;

        // Act
        profile.SetIsSearchable(true);

        // Assert
        Assert.True(profile.IsSearchable);
        Assert.NotEqual(oldModifiedOn, profile.ModifiedOn);
    }

    [Fact]
    public void SetEnabled_ShouldUpdateEnabled()
    {
        // Arrange
        var profile = CreateValidProfile();
        var oldModifiedOn = profile.ModifiedOn;

        // Act
        profile.SetEnabled(false);

        // Assert
        Assert.False(profile.Enabled);
        Assert.NotEqual(oldModifiedOn, profile.ModifiedOn);
    }

    private Profile CreateValidProfile()
    {
        return Profile.Create(
            _faker.Random.Guid().ToString(),
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        );
    }
}
