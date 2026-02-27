using Bogus;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.GetGroupCheckIns;
using HexMaster.Attendr.Groups.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.GetGroupCheckIns;

public class GetGroupCheckInsQueryHandlerTests
{
    private readonly Mock<ICheckInRepository> _checkInRepositoryMock;
    private readonly Mock<ILogger<GetGroupCheckInsQueryHandler>> _loggerMock;
    private readonly GetGroupCheckInsQueryHandler _handler;
    private readonly Faker _faker = new();

    public GetGroupCheckInsQueryHandlerTests()
    {
        _checkInRepositoryMock = new Mock<ICheckInRepository>();
        _loggerMock = new Mock<ILogger<GetGroupCheckInsQueryHandler>>();
        _handler = new GetGroupCheckInsQueryHandler(
            _checkInRepositoryMock.Object,
            _loggerMock.Object);
    }

    private static PresentationData CreatePresentationData()
    {
        return new PresentationData(
            Guid.NewGuid(),
            "Test presentation title",
            "Test abstract content",
            "Room A",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            Array.Empty<PresentationSpeaker>());
    }

    [Fact]
    public async Task Handle_WhenCheckInsExist_ShouldReturnMappedDtos()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var presentationData = CreatePresentationData();
        var checkIn = CheckIn.Create(
            groupId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            presentationData,
            DateTimeOffset.UtcNow.AddHours(2));
        checkIn.AddMember(new CheckedInMember(Guid.NewGuid(), "Member One", null));

        _checkInRepositoryMock
            .Setup(r => r.GetActiveByGroupAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheckIn> { checkIn });

        var query = new GetGroupCheckInsQuery(groupId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var dto = result.First();
        Assert.Equal(checkIn.Id, dto.Id);
        Assert.Equal(groupId, dto.GroupId);
        Assert.Single(dto.Members);
    }

    [Fact]
    public async Task Handle_WhenNoCheckIns_ShouldReturnEmptyList()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        _checkInRepositoryMock
            .Setup(r => r.GetActiveByGroupAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheckIn>());

        var query = new GetGroupCheckInsQuery(groupId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenCheckInHasSpeakers_ShouldMapSpeakersCorrectly()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var speaker = new PresentationSpeaker(Guid.NewGuid(), "Test Speaker", "https://example.com/pic.jpg");
        var presentationData = new PresentationData(
            Guid.NewGuid(),
            "Talk Title",
            "Abstract text here",
            "Main Hall",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            new[] { speaker });

        var checkIn = CheckIn.Create(
            groupId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            presentationData,
            DateTimeOffset.UtcNow.AddHours(2));

        _checkInRepositoryMock
            .Setup(r => r.GetActiveByGroupAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CheckIn> { checkIn });

        var query = new GetGroupCheckInsQuery(groupId);

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.Single(result);
        var dto = result.First();
        Assert.Single(dto.PresentationData.Speakers);
        Assert.Equal("Test Speaker", dto.PresentationData.Speakers.First().Name);
    }

    [Fact]
    public async Task Handle_WhenQueryIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!));
    }
}
