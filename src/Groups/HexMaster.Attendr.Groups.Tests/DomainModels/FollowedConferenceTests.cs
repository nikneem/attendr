using Bogus;
using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

public sealed class FollowedConferenceTests
{
    private readonly Faker _faker = new();

    private static FollowedConference CreateValid(
        Guid? conferenceId = null,
        string? name = null,
        string? city = null,
        string? country = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var start = startDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(12));

        return new FollowedConference(
            conferenceId ?? Guid.NewGuid(),
            name ?? "TechConf 2026",
            city ?? "Amsterdam",
            country ?? "Netherlands",
            null,
            10,
            20,
            start,
            end);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateConference()
    {
        // Arrange
        var id = Guid.NewGuid();
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        // Act
        var conf = new FollowedConference(id, "Some Conf", "City", "Country", "http://img", 5, 10, start, end);

        // Assert
        Assert.Equal(id, conf.ConferenceId);
        Assert.Equal("Some Conf", conf.Name);
        Assert.Equal("City", conf.City);
        Assert.Equal("Country", conf.Country);
        Assert.Equal("http://img", conf.ImageUrl);
        Assert.Equal(5, conf.SpeakersCount);
        Assert.Equal(10, conf.SessionsCount);
        Assert.Equal(start, conf.StartDate);
        Assert.Equal(end, conf.EndDate);
    }

    [Fact]
    public void Constructor_WithEmptyConferenceId_ShouldThrowArgumentException()
    {
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        Assert.Throws<ArgumentException>(() =>
            new FollowedConference(Guid.Empty, "Conf", "City", "Country", null, 0, 0, start, end));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        Assert.Throws<ArgumentException>(() =>
            new FollowedConference(Guid.NewGuid(), invalidName!, "City", "Country", null, 0, 0, start, end));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCity_ShouldThrowArgumentException(string? invalidCity)
    {
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        Assert.Throws<ArgumentException>(() =>
            new FollowedConference(Guid.NewGuid(), "Conf", invalidCity!, "Country", null, 0, 0, start, end));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCountry_ShouldThrowArgumentException(string? invalidCountry)
    {
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        Assert.Throws<ArgumentException>(() =>
            new FollowedConference(Guid.NewGuid(), "Conf", "City", invalidCountry!, null, 0, 0, start, end));
    }

    [Fact]
    public void Constructor_WhenStartDateAfterEndDate_ShouldThrowArgumentException()
    {
        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(12));
        var end = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

        Assert.Throws<ArgumentException>(() =>
            new FollowedConference(Guid.NewGuid(), "Conf", "City", "Country", null, 0, 0, start, end));
    }

    [Fact]
    public void GetLocation_ShouldReturnCityAndCountry()
    {
        var conf = CreateValid(city: "Berlin", country: "Germany");

        Assert.Equal("Berlin, Germany", conf.GetLocation());
    }

    [Fact]
    public void IsCurrentOrFuture_WhenEndDateIsInFuture_ShouldReturnTrue()
    {
        var conf = CreateValid(
            startDate: DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
            endDate: DateOnly.FromDateTime(DateTime.Today.AddDays(12)));

        Assert.True(conf.IsCurrentOrFuture());
    }

    [Fact]
    public void IsCurrentOrFuture_WhenEndDateIsToday_ShouldReturnTrue()
    {
        var conf = CreateValid(
            startDate: DateOnly.FromDateTime(DateTime.Today),
            endDate: DateOnly.FromDateTime(DateTime.Today));

        Assert.True(conf.IsCurrentOrFuture());
    }
}
