using Bogus;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.ProcessProfileCheckedIn;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Integrations.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.ProcessProfileCheckedIn;

public class ProcessProfileCheckedInCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepositoryMock;
    private readonly Mock<ICheckInRepository> _checkInRepositoryMock;
    private readonly Mock<IConferencesIntegrationService> _conferencesIntegrationMock;
    private readonly Mock<IProfilesIntegrationService> _profilesIntegrationMock;
    private readonly Mock<ILogger<ProcessProfileCheckedInCommandHandler>> _loggerMock;
    private readonly ProcessProfileCheckedInCommandHandler _handler;
    private readonly Faker _faker = new();

    public ProcessProfileCheckedInCommandHandlerTests()
    {
        _groupRepositoryMock = new Mock<IGroupRepository>();
        _checkInRepositoryMock = new Mock<ICheckInRepository>();
        _conferencesIntegrationMock = new Mock<IConferencesIntegrationService>();
        _profilesIntegrationMock = new Mock<IProfilesIntegrationService>();
        _loggerMock = new Mock<ILogger<ProcessProfileCheckedInCommandHandler>>();
        _handler = new ProcessProfileCheckedInCommandHandler(
            _groupRepositoryMock.Object,
            _checkInRepositoryMock.Object,
            _conferencesIntegrationMock.Object,
            _profilesIntegrationMock.Object,
            _loggerMock.Object);
    }

    private ProfileCheckedInEvent CreateCheckedInEvent(Guid profileId, bool isCheckedIn = true)
    {
        return new ProfileCheckedInEvent
        {
            ProfileId = profileId,
            ConferenceId = Guid.NewGuid(),
            PresentationId = Guid.NewGuid(),
            Title = "Test Presentation",
            Room = "Main Hall",
            StartDateTime = DateTimeOffset.UtcNow,
            EndDateTime = DateTimeOffset.UtcNow.AddHours(1),
            IsCheckedIn = isCheckedIn
        };
    }

    [Fact]
    public async Task Handle_WhenProfileNotMemberOfAnyGroup_ShouldReturnEarly()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group>());

        var command = new ProcessProfileCheckedInCommand(CreateCheckedInEvent(profileId));

        // Act
        await _handler.Handle(command);

        // Assert - no repository updates
        _checkInRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CheckIn>(), It.IsAny<CancellationToken>()), Times.Never);
        _groupRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<HexMaster.Attendr.Groups.DomainModels.Group>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCheckedIn_AndNoExistingCheckIn_AndPresentationFound_ShouldCreateNewCheckIn()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        var @event = CreateCheckedInEvent(profileId, isCheckedIn: true);

        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group });

        _profilesIntegrationMock
            .Setup(s => s.GetProfileDetails(profileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileDetailsDto(
                profileId.ToString(), "Test User", "Test", "User",
                "test@example.com", null, null, true));

        _checkInRepositoryMock
            .Setup(r => r.GetByGroupConferenceAndPresentationAsync(
                group.Id, @event.ConferenceId, @event.PresentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckIn?)null);

        var presentationDto = new PresentationDto(
            @event.PresentationId,
            @event.Title,
            "Abstract text",
            @event.StartDateTime,
            @event.EndDateTime,
            @event.Room,
            new List<SpeakerDto>(),
            new List<TopicReferenceDto>());

        _conferencesIntegrationMock
            .Setup(s => s.GetPresentationDetails(@event.ConferenceId, @event.PresentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(presentationDto);

        var command = new ProcessProfileCheckedInCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert
        _checkInRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CheckIn>(), It.IsAny<CancellationToken>()), Times.Once);
        _groupRepositoryMock.Verify(r => r.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCheckedIn_AndNoExistingCheckIn_AndPresentationNotFound_ShouldAttemptFallback()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        var @event = CreateCheckedInEvent(profileId, isCheckedIn: true);

        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group });

        _profilesIntegrationMock
            .Setup(s => s.GetProfileDetails(profileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileDetailsDto?)null);

        _checkInRepositoryMock
            .Setup(r => r.GetByGroupConferenceAndPresentationAsync(
                group.Id, @event.ConferenceId, @event.PresentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckIn?)null);

        // Presentation not found - handler will attempt fallback with empty abstract which throws
        _conferencesIntegrationMock
            .Setup(s => s.GetPresentationDetails(@event.ConferenceId, @event.PresentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PresentationDto?)null);

        var command = new ProcessProfileCheckedInCommand(@event);

        // Note: handler uses string.Empty for abstract in the fallback path, which violates
        // PresentationData's validation and throws ArgumentException (known handler limitation)
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command));
    }

    [Fact]
    public async Task Handle_WhenCheckedIn_AndExistingCheckIn_ShouldAddMemberToExistingCheckIn()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        var @event = CreateCheckedInEvent(profileId, isCheckedIn: true);

        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group });

        _profilesIntegrationMock
            .Setup(s => s.GetProfileDetails(profileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileDetailsDto(
                profileId.ToString(), "Test User", "Test", "User",
                "test@example.com", null, null, true));

        var existingCheckIn = CheckIn.Create(
            group.Id, @event.ConferenceId, @event.PresentationId,
            new PresentationData(@event.PresentationId, "Title", "Abstract", "Room",
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), Array.Empty<PresentationSpeaker>()),
            DateTimeOffset.UtcNow.AddHours(2));

        _checkInRepositoryMock
            .Setup(r => r.GetByGroupConferenceAndPresentationAsync(
                group.Id, @event.ConferenceId, @event.PresentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCheckIn);

        var command = new ProcessProfileCheckedInCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert
        _checkInRepositoryMock.Verify(r => r.AddMemberAsync(
            existingCheckIn.Id, It.IsAny<CheckedInMember>(), It.IsAny<CancellationToken>()), Times.Once);
        _checkInRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CheckIn>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCheckedOut_AndCheckInExists_ShouldRemoveMemberFromCheckIn()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        var @event = CreateCheckedInEvent(profileId, isCheckedIn: false);

        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group });

        _profilesIntegrationMock
            .Setup(s => s.GetProfileDetails(profileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileDetailsDto?)null);

        var existingCheckIn = CheckIn.Create(
            group.Id, @event.ConferenceId, @event.PresentationId,
            new PresentationData(@event.PresentationId, "Title", "Abstract", "Room",
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), Array.Empty<PresentationSpeaker>()),
            DateTimeOffset.UtcNow.AddHours(2));

        _checkInRepositoryMock
            .Setup(r => r.GetByGroupConferenceAndPresentationAsync(
                group.Id, @event.ConferenceId, @event.PresentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCheckIn);

        var command = new ProcessProfileCheckedInCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert
        _checkInRepositoryMock.Verify(r => r.RemoveMemberAsync(
            existingCheckIn.Id, profileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCheckedOut_AndNoCheckIn_ShouldDoNothing()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var group = GroupFactory.CreatePersistedGroup();
        var @event = CreateCheckedInEvent(profileId, isCheckedIn: false);

        _groupRepositoryMock
            .Setup(r => r.GetGroupsByMemberIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HexMaster.Attendr.Groups.DomainModels.Group> { group });

        _profilesIntegrationMock
            .Setup(s => s.GetProfileDetails(profileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileDetailsDto?)null);

        _checkInRepositoryMock
            .Setup(r => r.GetByGroupConferenceAndPresentationAsync(
                group.Id, @event.ConferenceId, @event.PresentationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckIn?)null);

        var command = new ProcessProfileCheckedInCommand(@event);

        // Act
        await _handler.Handle(command);

        // Assert
        _checkInRepositoryMock.Verify(r => r.RemoveMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCommandIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!));
    }
}
