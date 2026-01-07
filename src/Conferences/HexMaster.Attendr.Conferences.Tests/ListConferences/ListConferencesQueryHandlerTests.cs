using Bogus;
using HexMaster.Attendr.Conferences.ListConferences;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using HexMaster.Attendr.Core.Constants;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.ListConferences;

public sealed class ListConferencesQueryHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly ConferenceMetrics _metrics;
    private readonly Mock<ILogger<ListConferencesQueryHandler>> _mockLogger;
    private readonly ListConferencesQueryHandler _handler;
    private readonly Faker _faker;

    public ListConferencesQueryHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _metrics = TestMetricsFactory.CreateConferenceMetrics();
        _mockLogger = new Mock<ILogger<ListConferencesQueryHandler>>();
        _handler = new ListConferencesQueryHandler(
            _mockRepository.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithNoSearchQuery_ShouldReturnAllConferences()
    {
        // Arrange
        var conferences = new List<HexMaster.Attendr.Conferences.DomainModels.Conference>
        {
            ConferenceFactory.CreatePersistedConference(),
            ConferenceFactory.CreatePersistedConference(),
            ConferenceFactory.CreatePersistedConference()
        };
        var totalCount = 3;
        var query = new ListConferencesQuery();

        _mockRepository.Setup(x => x.ListConferencesAsync(
            null,
            1,
            PaginationConstants.DefaultPageSize,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((conferences, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Conferences.Count);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(PaginationConstants.DefaultPageSize, result.PageSize);

        _mockRepository.Verify(x => x.ListConferencesAsync(
            null,
            1,
            PaginationConstants.DefaultPageSize,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSearchQuery_ShouldReturnFilteredConferences()
    {
        // Arrange
        var searchQuery = "NDC";
        var conferences = new List<HexMaster.Attendr.Conferences.DomainModels.Conference>
        {
            ConferenceFactory.CreatePersistedConference()
        };
        var totalCount = 1;
        var query = new ListConferencesQuery(SearchQuery: searchQuery);

        _mockRepository.Setup(x => x.ListConferencesAsync(
            searchQuery,
            1,
            PaginationConstants.DefaultPageSize,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((conferences, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Conferences);
        Assert.Equal(totalCount, result.TotalCount);

        _mockRepository.Verify(x => x.ListConferencesAsync(
            searchQuery,
            1,
            PaginationConstants.DefaultPageSize,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomPageSize_ShouldRespectPageSize()
    {
        // Arrange
        var pageSize = 5;
        var conferences = new List<HexMaster.Attendr.Conferences.DomainModels.Conference>
        {
            ConferenceFactory.CreatePersistedConference(),
            ConferenceFactory.CreatePersistedConference()
        };
        var totalCount = 10;
        var query = new ListConferencesQuery(PageSize: pageSize, PageNumber: 2);

        _mockRepository.Setup(x => x.ListConferencesAsync(
            null,
            2,
            pageSize,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((conferences, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Conferences.Count);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);

        _mockRepository.Verify(x => x.ListConferencesAsync(
            null,
            2,
            pageSize,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoConferencesFound_ShouldReturnEmptyList()
    {
        // Arrange
        var conferences = new List<HexMaster.Attendr.Conferences.DomainModels.Conference>();
        var totalCount = 0;
        var query = new ListConferencesQuery();

        _mockRepository.Setup(x => x.ListConferencesAsync(
            null,
            1,
            PaginationConstants.DefaultPageSize,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((conferences, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Conferences);
        Assert.Equal(0, result.TotalCount);

        _mockRepository.Verify(x => x.ListConferencesAsync(
            null,
            1,
            PaginationConstants.DefaultPageSize,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithConferencesHavingSpeakersAndRooms_ShouldIncludeCounts()
    {
        // Arrange
        var conference = ConferenceFactory.CreatePersistedConference();
        var room = ConferenceFactory.CreateRoom();
        var room2 = ConferenceFactory.CreateRoom();
        var speaker = ConferenceFactory.CreateSpeaker();
        conference.AddRoom(room);
        conference.AddRoom(room2);
        conference.AddSpeaker(speaker);

        var presentation = ConferenceFactory.CreatePresentation();
        // Don't add presentation because it requires room/speaker validation

        var conferences = new List<HexMaster.Attendr.Conferences.DomainModels.Conference> { conference };
        var totalCount = 1;
        var query = new ListConferencesQuery();

        _mockRepository.Setup(x => x.ListConferencesAsync(
            null,
            1,
            PaginationConstants.DefaultPageSize,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((conferences, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Conferences);
        var dto = result.Conferences.First();
        Assert.Equal(1, dto.SpeakersCount);
        Assert.Equal(2, dto.RoomsCount);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var query = new ListConferencesQuery();

        _mockRepository.Setup(x => x.ListConferencesAsync(
            null,
            1,
            PaginationConstants.DefaultPageSize,
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
