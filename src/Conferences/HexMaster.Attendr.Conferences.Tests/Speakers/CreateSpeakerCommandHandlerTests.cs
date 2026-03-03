using Bogus;
using HexMaster.Attendr.Conferences.Features.Speakers.CreateSpeaker;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.Speakers;

public sealed class CreateSpeakerCommandHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<ILogger<CreateSpeakerCommandHandler>> _mockLogger;
    private readonly CreateSpeakerCommandHandler _handler;
    private readonly Faker _faker;

    public CreateSpeakerCommandHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockLogger = new Mock<ILogger<CreateSpeakerCommandHandler>>();
        _handler = new CreateSpeakerCommandHandler(_mockRepository.Object, _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_AsCreator_ShouldCreateSpeakerAndMarkInvisible()
    {
        var profileId = Guid.NewGuid();
        var conference = ConferenceFactory.CreatePersistedConference(createdByProfileId: profileId);
        conference.UpdateVisibility(true);
        _mockRepository.Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var command = new CreateSpeakerCommand(conference.Id, _faker.Name.FullName(), null, null, profileId, false);
        var result = await _handler.Handle(command);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.False(conference.IsVisible);
        _mockRepository.Verify(r => r.UpdateAsync(conference, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldCreateSpeaker()
    {
        var conference = ConferenceFactory.CreatePersistedConference(createdByProfileId: Guid.NewGuid());
        _mockRepository.Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var command = new CreateSpeakerCommand(conference.Id, _faker.Name.FullName(), null, null, Guid.NewGuid(), true);
        var result = await _handler.Handle(command);

        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Handle_AsNonCreatorNonAdmin_ShouldThrowForbidden()
    {
        var conference = ConferenceFactory.CreatePersistedConference(createdByProfileId: Guid.NewGuid());
        _mockRepository.Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var command = new CreateSpeakerCommand(conference.Id, _faker.Name.FullName(), null, null, Guid.NewGuid(), false);
        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenConferenceNotFound_ShouldThrowKeyNotFoundException()
    {
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HexMaster.Attendr.Conferences.DomainModels.Conference?)null);

        var command = new CreateSpeakerCommand(Guid.NewGuid(), "Speaker", null, null, Guid.NewGuid(), false);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command));
    }
}
