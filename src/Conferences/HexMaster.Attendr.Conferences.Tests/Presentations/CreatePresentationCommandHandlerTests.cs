using Bogus;
using HexMaster.Attendr.Conferences.Features.Presentations.CreatePresentation;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.Presentations;

public sealed class CreatePresentationCommandHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<ILogger<CreatePresentationCommandHandler>> _mockLogger;
    private readonly CreatePresentationCommandHandler _handler;
    private readonly Faker _faker;

    public CreatePresentationCommandHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockLogger = new Mock<ILogger<CreatePresentationCommandHandler>>();
        _handler = new CreatePresentationCommandHandler(_mockRepository.Object, _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_AsCreator_ShouldCreatePresentationAndMarkInvisible()
    {
        var profileId = Guid.NewGuid();
        var conference = ConferenceFactory.CreatePersistedConference(createdByProfileId: profileId);
        conference.UpdateVisibility(true);
        var room = ConferenceFactory.CreateRoom();
        conference.AddRoom(room);
        var speaker = ConferenceFactory.CreateSpeaker();
        conference.AddSpeaker(speaker);

        _mockRepository.Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var start = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreatePresentationCommand(
            conference.Id, "Test Talk", "Abstract text",
            start, start.AddHours(1), room.Id, new[] { speaker.Id },
            profileId, false);

        var result = await _handler.Handle(command);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(room.Id, result.RoomId);
        Assert.Contains(speaker.Id, result.SpeakerIds);
        Assert.False(conference.IsVisible);
        _mockRepository.Verify(r => r.UpdateAsync(conference, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoSpeakers_ShouldThrowArgumentException()
    {
        var conference = ConferenceFactory.CreatePersistedConference(createdByProfileId: Guid.NewGuid());
        var room = ConferenceFactory.CreateRoom();
        conference.AddRoom(room);

        _mockRepository.Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var start = DateTimeOffset.UtcNow.AddDays(1);
        var profileId = conference.CreatedByProfileId!.Value;
        var command = new CreatePresentationCommand(
            conference.Id, "Test Talk", "Abstract",
            start, start.AddHours(1), room.Id, Array.Empty<Guid>(),
            profileId, false);

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command));
    }

    [Fact]
    public async Task Handle_AsNonCreatorNonAdmin_ShouldThrowForbidden()
    {
        var conference = ConferenceFactory.CreatePersistedConference(createdByProfileId: Guid.NewGuid());
        _mockRepository.Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var start = DateTimeOffset.UtcNow.AddDays(1);
        var command = new CreatePresentationCommand(
            conference.Id, "Talk", "Abstract",
            start, start.AddHours(1), Guid.NewGuid(), new[] { Guid.NewGuid() },
            Guid.NewGuid(), false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command));
    }
}
