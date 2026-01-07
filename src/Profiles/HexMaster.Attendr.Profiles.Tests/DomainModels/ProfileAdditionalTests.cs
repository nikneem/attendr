using Bogus;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Tests.Factories;

namespace HexMaster.Attendr.Profiles.Tests.DomainModels;

public class ProfileAdditionalTests
{
    private readonly Faker _faker;

    public ProfileAdditionalTests()
    {
        _faker = new Faker();
    }

    [Fact]
    public void Create_ShouldSetStateToCreated()
    {
        // Arrange & Act
        var profile = ProfileFactory.CreateProfile();

        // Assert
        Assert.Equal(Core.DomainModels.DomainModelState.Created, profile.State);
    }

    [Fact]
    public void Create_ShouldSetIsSearchableToFalse()
    {
        // Arrange & Act
        var profile = ProfileFactory.CreateProfile();

        // Assert
        Assert.False(profile.IsSearchable);
    }

    [Fact]
    public void Create_ShouldSetEnabledToTrue()
    {
        // Arrange & Act
        var profile = ProfileFactory.CreateProfile();

        // Assert
        Assert.True(profile.Enabled);
    }

    [Fact]
    public void FromPersisted_ShouldSetStateToPristine()
    {
        // Arrange & Act
        var profile = ProfileFactory.CreatePersistedProfile();

        // Assert
        Assert.Equal(Core.DomainModels.DomainModelState.Pristine, profile.State);
    }

    [Fact]
    public void SetEmail_ShouldConvertToLowercase()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();
        var upperCaseEmail = "TEST@EXAMPLE.COM";

        // Act
        profile.SetEmail(upperCaseEmail);

        // Assert
        Assert.Equal("test@example.com", profile.Email);
    }

    [Fact]
    public void SetEmail_ShouldThrowArgumentException_WhenMissingDot()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetEmail("test@example"));
    }

    [Fact]
    public void SetEmail_ShouldThrowArgumentException_WhenNull()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetEmail(null!));
    }

    [Fact]
    public void SetFirstName_ShouldThrowArgumentException_WhenNull()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetFirstName(null!));
    }

    [Fact]
    public void SetFirstName_ShouldThrowArgumentException_WhenWhitespace()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetFirstName("   "));
    }

    [Fact]
    public void SetLastName_ShouldThrowArgumentException_WhenEmpty()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetLastName(string.Empty));
    }

    [Fact]
    public void SetLastName_ShouldThrowArgumentException_WhenNull()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetLastName(null!));
    }

    [Fact]
    public void SetDisplayName_ShouldThrowArgumentException_WhenEmpty()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetDisplayName(string.Empty));
    }

    [Fact]
    public void SetDisplayName_ShouldThrowArgumentException_WhenNull()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetDisplayName(null!));
    }

    [Fact]
    public void SetEmployee_ShouldThrowArgumentException_WhenEmpty()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetEmployee(string.Empty));
    }

    [Fact]
    public void SetEmployee_ShouldThrowArgumentException_WhenNull()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetEmployee(null!));
    }

    [Fact]
    public void SetSubjectId_ShouldUpdateSubjectId_WithValidValue()
    {
        // Arrange
        var profile = ProfileFactory.CreatePersistedProfile();
        var newSubjectId = _faker.Random.Guid().ToString();

        // Act
        profile.SetSubjectId(newSubjectId);

        // Assert
        Assert.Equal(newSubjectId, profile.SubjectId);
        Assert.NotNull(profile.ModifiedOn);
    }

    [Fact]
    public void SetSubjectId_ShouldThrowArgumentException_WhenEmpty()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetSubjectId(string.Empty));
    }

    [Fact]
    public void SetSubjectId_ShouldThrowArgumentException_WhenNull()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetSubjectId(null!));
    }

    [Fact]
    public void SetTagLine_ShouldSetEmptyString_WhenEmpty()
    {
        // Arrange
        var profile = ProfileFactory.CreatePersistedProfile(tagLine: "Original TagLine");

        // Act
        profile.SetTagLine(string.Empty);

        // Assert
        Assert.Equal(string.Empty, profile.TagLine);
    }

    [Fact]
    public void SetTagLine_ShouldSetEmptyString_WhenWhitespace()
    {
        // Arrange
        var profile = ProfileFactory.CreatePersistedProfile(tagLine: "Original TagLine");

        // Act
        profile.SetTagLine("   ");

        // Assert
        Assert.Equal(string.Empty, profile.TagLine);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenSubjectIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Profile.Create(
            string.Empty,
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        ));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenDisplayNameIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Profile.Create(
            _faker.Random.Guid().ToString(),
            string.Empty,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        ));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenEmailIsInvalid()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Profile.Create(
            _faker.Random.Guid().ToString(),
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            "invalid-email"
        ));
    }

    [Fact]
    public void FromPersisted_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Profile.FromPersisted(
            string.Empty,
            _faker.Random.Guid().ToString(),
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email,
            null,
            null,
            true,
            false
        ));
    }

    [Fact]
    public void SetEmail_ShouldThrowArgumentException_WhenWhitespace()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetEmail("   "));
    }

    [Fact]
    public void SetDisplayName_ShouldThrowArgumentException_WhenWhitespace()
    {
        // Arrange
        var profile = ProfileFactory.CreateProfile();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => profile.SetDisplayName("   "));
    }
}
