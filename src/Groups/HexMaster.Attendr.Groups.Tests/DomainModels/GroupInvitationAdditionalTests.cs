using Bogus;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

/// <summary>
/// Tests for GroupInvitation covering cases not in existing tests.
/// </summary>
public sealed class GroupInvitationAdditionalTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateInvitation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = _faker.Person.FullName;
        const string code = "ABCD1234";
        var expiration = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var invitation = new GroupInvitation(id, name, code, expiration);

        // Assert
        Assert.Equal(id, invitation.Id);
        Assert.Equal(name, invitation.Name);
        Assert.Equal("ABCD1234", invitation.AcceptanceCode); // stored uppercase
        Assert.Equal(expiration, invitation.ExpirationDate);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new GroupInvitation(Guid.Empty, _faker.Person.FullName, "ABCD1234", DateTimeOffset.UtcNow.AddDays(1)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        Assert.Throws<ArgumentException>(() =>
            new GroupInvitation(Guid.NewGuid(), invalidName!, "ABCD1234", DateTimeOffset.UtcNow.AddDays(1)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidAcceptanceCode_ShouldThrowArgumentException(string? invalidCode)
    {
        Assert.Throws<ArgumentException>(() =>
            new GroupInvitation(Guid.NewGuid(), _faker.Person.FullName, invalidCode!, DateTimeOffset.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Constructor_WithCodeThatIsNot8Chars_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new GroupInvitation(Guid.NewGuid(), _faker.Person.FullName, "SHORT", DateTimeOffset.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Constructor_WithPastExpirationDate_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new GroupInvitation(Guid.NewGuid(), _faker.Person.FullName, "ABCD1234", DateTimeOffset.UtcNow.AddDays(-1)));
    }

    [Fact]
    public void AcceptanceCode_ShouldBeStoredAsUppercase()
    {
        var invitation = new GroupInvitation(Guid.NewGuid(), _faker.Person.FullName, "abcd1234", DateTimeOffset.UtcNow.AddDays(1));

        Assert.Equal("ABCD1234", invitation.AcceptanceCode);
    }

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        var invitation = new GroupInvitation(Guid.NewGuid(), _faker.Person.FullName, "ABCD1234", DateTimeOffset.UtcNow.AddDays(7));

        Assert.False(invitation.IsExpired());
    }

    [Fact]
    public void GenerateAcceptanceCode_ShouldReturn8CharCode()
    {
        var code = GroupInvitation.GenerateAcceptanceCode();

        Assert.Equal(8, code.Length);
        Assert.True(code == code.ToUpperInvariant(), "Code should be uppercase");
    }
}
