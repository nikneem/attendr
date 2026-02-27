using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.UpdateConference;
using HexMaster.Attendr.Presence.Tests.Factories;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.UpdateConference;

public sealed class UpdateConferenceCommandHandlerTests
{
    private readonly Mock<IConferencePresenceRepository> _repositoryMock;
    private readonly UpdateConferenceCommandHandler _sut;

    public UpdateConferenceCommandHandlerTests()
    {
        _repositoryMock = new Mock<IConferencePresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();
        _sut = new UpdateConferenceCommandHandler(
            _repositoryMock.Object,
            metrics,
            NullLogger<UpdateConferenceCommandHandler>.Instance);
    }

    private static ConferenceUpdatedEvent BuildEvent(Guid conferenceId) =>
        new()
        {
            ConferenceId = conferenceId,
            Title = "Updated Conference",
            City = "Rotterdam",
            Country = "Netherlands",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(32)),
            ImageUrl = null
        };

    [Fact]
    public async Task Handle_WhenPresencesExist_ShouldUpdateAllPresences()
    {
        var conferenceId = Guid.NewGuid();
        var presences = ConferencePresenceFactory.CreateList(3);

        _repositoryMock
            .Setup(r => r.GetByConferenceIdAsync(conferenceId, default))
            .ReturnsAsync(presences.AsReadOnly());

        var command = new UpdateConferenceCommand(BuildEvent(conferenceId));

        await _sut.Handle(command);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ConferencePresence>(), default), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_WhenNoPresencesExist_ShouldReturnWithoutUpdating()
    {
        var conferenceId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByConferenceIdAsync(conferenceId, default))
            .ReturnsAsync(new List<ConferencePresence>().AsReadOnly());

        var command = new UpdateConferenceCommand(BuildEvent(conferenceId));

        await _sut.Handle(command);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ConferencePresence>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateConferenceDetailsOnEachPresence()
    {
        var conferenceId = Guid.NewGuid();
        var presence = ConferencePresenceFactory.Create(conferenceId: conferenceId);

        _repositoryMock
            .Setup(r => r.GetByConferenceIdAsync(conferenceId, default))
            .ReturnsAsync(new List<ConferencePresence> { presence }.AsReadOnly());

        var @event = BuildEvent(conferenceId);
        var command = new UpdateConferenceCommand(@event);

        await _sut.Handle(command);

        Assert.Equal(@event.Title, presence.ConferenceName);
    }
}
