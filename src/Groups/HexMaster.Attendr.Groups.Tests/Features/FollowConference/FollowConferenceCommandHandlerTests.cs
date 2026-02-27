using Bogus;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Integrations.Abstractions;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.FollowConference;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.FollowConference;

public sealed class FollowConferenceCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly Mock<IConferencesIntegrationService> _mockConferencesIntegration;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<FollowConferenceCommandHandler>> _mockLogger;
    private readonly FollowConferenceCommandHandler _handler;
    private readonly Faker _faker;

    public FollowConferenceCommandHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _mockConferencesIntegration = new Mock<IConferencesIntegrationService>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<FollowConferenceCommandHandler>>();
        _handler = new FollowConferenceCommandHandler(
            _mockRepository.Object,
            _mockConferencesIntegration.Object,
            _mockEventPublisher.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    private static ConferenceDetailsDto CreateConferenceDto(Guid conferenceId)
    {
        return new ConferenceDetailsDto(
            conferenceId,
            "TechConf 2026",
            "Amsterdam",
            "Netherlands",
            DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(12)),
            null,
            true,
            null,
            new List<SpeakerDto>(),
            new List<PresentationDto>());
    }

    [Fact]
    public async Task Handle_WhenMemberFollowsConference_ShouldAddConference()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var conferenceId = Guid.NewGuid();
        var conferenceDto = CreateConferenceDto(conferenceId);

        var command = new FollowConferenceCommand(group.Id, conferenceId, ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockConferencesIntegration.Setup(x => x.GetConferenceDetails(conferenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conferenceDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains(group.FollowedConferences, fc => fc.ConferenceId == conferenceId);
        _mockRepository.Verify(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowInvalidOperationException()
    {
        var command = new FollowConferenceCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberNotInGroup_ShouldThrowInvalidOperationException()
    {
        var group = GroupFactory.CreateGroup();
        var command = new FollowConferenceCommand(group.Id, Guid.NewGuid(), Guid.NewGuid());

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenConferenceNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var conferenceId = Guid.NewGuid();

        var command = new FollowConferenceCommand(group.Id, conferenceId, ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockConferencesIntegration.Setup(x => x.GetConferenceDetails(conferenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConferenceDetailsDto?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!, CancellationToken.None));
    }
}
