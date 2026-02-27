using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Features.GetConference;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.GetConference;

public class GetConferenceQueryHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<ILogger<GetConferenceQueryHandler>> _mockLogger;
    private readonly GetConferenceQueryHandler _handler;

    public GetConferenceQueryHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockLogger = new Mock<ILogger<GetConferenceQueryHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new GetConferenceQueryHandler(
            _mockRepository.Object,
            metrics,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenConferenceExists_ShouldReturnConferenceDetails()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var conferenceDetails = new ConferenceDetailsDto(
            conferenceId,
            "Test Conference",
            "Amsterdam",
            "Netherlands",
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1).AddDays(3)),
            "https://test.com/image.jpg",
            false,
            null,
            new List<SpeakerDto>(),
            new List<PresentationDto>());

        _mockRepository.Setup(x => x.GetDetailsByIdAsync(conferenceId, It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conferenceDetails);

        var query = new GetConferenceQuery(conferenceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(conferenceId, result.Id);
        Assert.Equal("Test Conference", result.Title);
        _mockRepository.Verify(x => x.GetDetailsByIdAsync(conferenceId, It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConferenceDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetDetailsByIdAsync(conferenceId, It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConferenceDetailsDto?)null);

        var query = new GetConferenceQuery(conferenceId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.GetDetailsByIdAsync(conferenceId, It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetDetailsByIdAsync(conferenceId, It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var query = new GetConferenceQuery(conferenceId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
