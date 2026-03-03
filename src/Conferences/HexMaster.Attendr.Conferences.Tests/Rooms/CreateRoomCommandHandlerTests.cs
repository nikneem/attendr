using Bogus;
using HexMaster.Attendr.Conferences.Features.Rooms.CreateRoom;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.Rooms;

public sealed class CreateRoomCommandHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<ILogger<CreateRoomCommandHandler>> _mockLogger;
    private readonly CreateRoomCommandHandler _handler;
    private readonly Faker _faker;

    public CreateRoomCommandHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockLogger = new Mock<ILogger<CreateRoomCommandHandler>>();
        _handler = new CreateRoomCommandHandler(_mockRepository.Object, _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_AsCreator_ShouldCreateRoomAndMarkInvisible()
    {
        var profileId = Guid.NewGuid();
        var conference = ConferenceFactory.CreatePersistedConference(createdByProfileId: profileId);
        conference.UpdateVisibility(true);
        _mockRepository.Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var command = new CreateRoomCommand(conference.Id, "Main Hall", 200, profileId, false);
        var result = await _handler.Handle(command);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Main Hall", result.Name);
        Assert.Equal(200, result.Capacity);
        Assert.False(conference.IsVisible);
        _mockRepository.Verify(r => r.UpdateAsync(conference, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AsNonCreatorNonAdmin_ShouldThrowForbidden()
    {
        var conference = ConferenceFactory.CreatePersistedConference(createdByProfileId: Guid.NewGuid());
        _mockRepository.Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var command = new CreateRoomCommand(conference.Id, "Room", 50, Guid.NewGuid(), false);
        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command));
    }
}
