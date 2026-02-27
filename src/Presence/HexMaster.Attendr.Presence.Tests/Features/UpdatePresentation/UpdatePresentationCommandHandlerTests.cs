using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Models;
using HexMaster.Attendr.IntegrationEvents.Services;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.UpdatePresentation;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.UpdatePresentation;

public sealed class UpdatePresentationCommandHandlerTests
{
    private readonly Mock<IPresentationPresenceRepository> _presRepositoryMock;
    private readonly Mock<IConferencePresenceRepository> _confRepositoryMock;
    private readonly Mock<IIntegrationEventPublisher> _publisherMock;
    private readonly UpdatePresentationCommandHandler _sut;

    public UpdatePresentationCommandHandlerTests()
    {
        _presRepositoryMock = new Mock<IPresentationPresenceRepository>();
        _confRepositoryMock = new Mock<IConferencePresenceRepository>();
        _publisherMock = new Mock<IIntegrationEventPublisher>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new UpdatePresentationCommandHandler(
            _presRepositoryMock.Object,
            _confRepositoryMock.Object,
            _publisherMock.Object,
            metrics,
            NullLogger<UpdatePresentationCommandHandler>.Instance);
    }

    private static PresentationUpdatedEvent BuildEvent(Guid conferenceId, Guid presentationId, bool scheduleChanged = false) =>
        new()
        {
            ConferenceId = conferenceId,
            PresentationId = presentationId,
            Title = "New Title",
            Abstract = "New Abstract",
            RoomName = "Room B",
            StartDateTime = DateTimeOffset.UtcNow.AddHours(2),
            EndDateTime = DateTimeOffset.UtcNow.AddHours(3),
            IsScheduleChanged = scheduleChanged,
            Speakers = Array.Empty<SpeakerDto>(),
            Topics = Array.Empty<PresentationTopicDto>()
        };

    [Fact]
    public async Task Handle_WhenNoConferencePresences_ShouldReturnEarly()
    {
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();

        _confRepositoryMock
            .Setup(r => r.GetByConferenceIdAsync(conferenceId, default))
            .ReturnsAsync(new List<ConferencePresence>().AsReadOnly());

        var command = new UpdatePresentationCommand(BuildEvent(conferenceId, presentationId));

        await _sut.Handle(command);

        _presRepositoryMock.Verify(r =>
            r.GetByConferenceAndPresentationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPresentationNotExists_ShouldCreateNew()
    {
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();
        var conferencePresences = new List<ConferencePresence> { ConferencePresenceFactory.Create(conferenceId: conferenceId) };

        _confRepositoryMock
            .Setup(r => r.GetByConferenceIdAsync(conferenceId, default))
            .ReturnsAsync(conferencePresences.AsReadOnly());
        _presRepositoryMock
            .Setup(r => r.GetByConferenceAndPresentationAsync(It.IsAny<Guid>(), conferenceId, presentationId, default))
            .ReturnsAsync((PresentationPresence?)null);

        var command = new UpdatePresentationCommand(BuildEvent(conferenceId, presentationId));

        await _sut.Handle(command);

        _presRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PresentationPresence>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPresentationExists_ShouldUpdateExisting()
    {
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var conferencePresence = ConferencePresenceFactory.Create(profileId: profileId, conferenceId: conferenceId);
        var presentation = PresentationPresenceFactory.Create(profileId: profileId, conferenceId: conferenceId, presentationId: presentationId);

        _confRepositoryMock
            .Setup(r => r.GetByConferenceIdAsync(conferenceId, default))
            .ReturnsAsync(new List<ConferencePresence> { conferencePresence }.AsReadOnly());
        _presRepositoryMock
            .Setup(r => r.GetByConferenceAndPresentationAsync(profileId, conferenceId, presentationId, default))
            .ReturnsAsync(presentation);

        var command = new UpdatePresentationCommand(BuildEvent(conferenceId, presentationId));

        await _sut.Handle(command);

        _presRepositoryMock.Verify(r =>
            r.UpdateAsync(profileId, conferenceId, presentation, default), Times.Once);
        Assert.Equal("New Title", presentation.Title);
    }

    [Fact]
    public async Task Handle_WhenScheduleChangedAndIsFavorite_ShouldPublishEvent()
    {
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var conferencePresence = ConferencePresenceFactory.Create(profileId: profileId, conferenceId: conferenceId);
        var presentation = PresentationPresenceFactory.Create(
            profileId: profileId, conferenceId: conferenceId, presentationId: presentationId,
            isFavorite: true);

        _confRepositoryMock
            .Setup(r => r.GetByConferenceIdAsync(conferenceId, default))
            .ReturnsAsync(new List<ConferencePresence> { conferencePresence }.AsReadOnly());
        _presRepositoryMock
            .Setup(r => r.GetByConferenceAndPresentationAsync(profileId, conferenceId, presentationId, default))
            .ReturnsAsync(presentation);

        var command = new UpdatePresentationCommand(BuildEvent(conferenceId, presentationId, scheduleChanged: true));

        await _sut.Handle(command);

        _publisherMock.Verify(p =>
            p.PublishAsync(It.IsAny<PresentationScheduleChangeEvent>(), default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenScheduleChangedButNotFavorite_ShouldNotPublishEvent()
    {
        var conferenceId = Guid.NewGuid();
        var presentationId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var conferencePresence = ConferencePresenceFactory.Create(profileId: profileId, conferenceId: conferenceId);
        var presentation = PresentationPresenceFactory.Create(
            profileId: profileId, conferenceId: conferenceId, presentationId: presentationId,
            isFavorite: false);

        _confRepositoryMock
            .Setup(r => r.GetByConferenceIdAsync(conferenceId, default))
            .ReturnsAsync(new List<ConferencePresence> { conferencePresence }.AsReadOnly());
        _presRepositoryMock
            .Setup(r => r.GetByConferenceAndPresentationAsync(profileId, conferenceId, presentationId, default))
            .ReturnsAsync(presentation);

        var command = new UpdatePresentationCommand(BuildEvent(conferenceId, presentationId, scheduleChanged: true));

        await _sut.Handle(command);

        _publisherMock.Verify(p =>
            p.PublishAsync(It.IsAny<PresentationScheduleChangeEvent>(), default),
            Times.Never);
    }
}
