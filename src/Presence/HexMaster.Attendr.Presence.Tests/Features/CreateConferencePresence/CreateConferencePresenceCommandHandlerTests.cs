using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Features.CreateConferencePresence;
using HexMaster.Attendr.Presence.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Presence.Tests.Features.CreateConferencePresence;

public sealed class CreateConferencePresenceCommandHandlerTests
{
    private readonly Mock<IConferencesIntegrationService> _integrationMock;
    private readonly Mock<IConferencePresenceRepository> _conferenceMock;
    private readonly Mock<IPresentationPresenceRepository> _presentationMock;
    private readonly CreateConferencePresenceCommandHandler _sut;

    private static readonly Guid _conferenceId = Guid.NewGuid();

    public CreateConferencePresenceCommandHandlerTests()
    {
        _integrationMock = new Mock<IConferencesIntegrationService>();
        _conferenceMock = new Mock<IConferencePresenceRepository>();
        _presentationMock = new Mock<IPresentationPresenceRepository>();
        var metrics = TestMetricsFactory.CreatePresenceMetrics();

        _sut = new CreateConferencePresenceCommandHandler(
            _integrationMock.Object,
            _conferenceMock.Object,
            _presentationMock.Object,
            metrics,
            NullLogger<CreateConferencePresenceCommandHandler>.Instance);
    }

    private static ConferenceDetailsDto BuildConferenceDetails(Guid? id = null)
    {
        var now = DateTimeOffset.UtcNow;
        var speakerId = Guid.NewGuid();
        return new ConferenceDetailsDto(
            id ?? _conferenceId,
            "Dev Summit",
            "Amsterdam",
            "Netherlands",
            DateOnly.FromDateTime(now.Date),
            DateOnly.FromDateTime(now.Date.AddDays(1)),
            null,
            true,
            null,
            new List<SpeakerDto>
            {
                new(speakerId, "John Doe", null)
            },
            new List<PresentationDto>
            {
                new(
                    Guid.NewGuid(),
                    "Keynote",
                    "Opening remarks",
                    now.AddHours(1),
                    now.AddHours(2),
                    "Main Hall",
                    new List<SpeakerDto> { new(speakerId, "John Doe", null) },
                    new List<TopicReferenceDto> { new("dotnet", ".NET") })
            });
    }

    [Fact]
    public async Task Handle_WhenConferenceNotFound_ShouldThrowInvalidOperationException()
    {
        _integrationMock
            .Setup(s => s.GetConferenceDetails(_conferenceId, default))
            .ReturnsAsync((ConferenceDetailsDto?)null);

        var command = new CreateConferencePresenceCommand(_conferenceId, new[] { Guid.NewGuid() });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenProfileAlreadyExists_ShouldSkipCreation()
    {
        var profileId = Guid.NewGuid();
        _integrationMock
            .Setup(s => s.GetConferenceDetails(_conferenceId, default))
            .ReturnsAsync(BuildConferenceDetails());
        _conferenceMock
            .Setup(r => r.ExistsAsync(profileId, _conferenceId, default))
            .ReturnsAsync(true);

        var command = new CreateConferencePresenceCommand(_conferenceId, new[] { profileId });

        await _sut.Handle(command);

        _conferenceMock.Verify(r => r.AddAsync(It.IsAny<ConferencePresence>(), default), Times.Never);
        _presentationMock.Verify(r => r.AddManyAsync(It.IsAny<IEnumerable<PresentationPresence>>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNewProfile_ShouldCreateConferenceAndPresentationPresences()
    {
        var profileId = Guid.NewGuid();
        _integrationMock
            .Setup(s => s.GetConferenceDetails(_conferenceId, default))
            .ReturnsAsync(BuildConferenceDetails());
        _conferenceMock
            .Setup(r => r.ExistsAsync(profileId, _conferenceId, default))
            .ReturnsAsync(false);

        var command = new CreateConferencePresenceCommand(_conferenceId, new[] { profileId });

        await _sut.Handle(command);

        _conferenceMock.Verify(r => r.AddAsync(It.IsAny<ConferencePresence>(), default), Times.Once);
        _presentationMock.Verify(r => r.AddManyAsync(It.IsAny<IEnumerable<PresentationPresence>>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleProfiles_ShouldCreateForEachProfile()
    {
        var profile1 = Guid.NewGuid();
        var profile2 = Guid.NewGuid();
        _integrationMock
            .Setup(s => s.GetConferenceDetails(_conferenceId, default))
            .ReturnsAsync(BuildConferenceDetails());
        _conferenceMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), _conferenceId, default))
            .ReturnsAsync(false);

        var command = new CreateConferencePresenceCommand(_conferenceId, new[] { profile1, profile2 });

        await _sut.Handle(command);

        _conferenceMock.Verify(r => r.AddAsync(It.IsAny<ConferencePresence>(), default), Times.Exactly(2));
        _presentationMock.Verify(r => r.AddManyAsync(It.IsAny<IEnumerable<PresentationPresence>>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenConferenceHasNoPresentations_ShouldNotCallAddMany()
    {
        var profileId = Guid.NewGuid();
        var details = new ConferenceDetailsDto(
            _conferenceId, "Empty Conf", "City", "Country",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            null, true, null,
            new List<SpeakerDto>(),
            new List<PresentationDto>());

        _integrationMock
            .Setup(s => s.GetConferenceDetails(_conferenceId, default))
            .ReturnsAsync(details);
        _conferenceMock
            .Setup(r => r.ExistsAsync(profileId, _conferenceId, default))
            .ReturnsAsync(false);

        var command = new CreateConferencePresenceCommand(_conferenceId, new[] { profileId });

        await _sut.Handle(command);

        _conferenceMock.Verify(r => r.AddAsync(It.IsAny<ConferencePresence>(), default), Times.Once);
        _presentationMock.Verify(r => r.AddManyAsync(It.IsAny<IEnumerable<PresentationPresence>>(), default), Times.Never);
    }
}
