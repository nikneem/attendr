using Bogus;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.JoinGroup;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.JoinGroup;

public sealed class JoinGroupCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<JoinGroupCommandHandler>> _mockLogger;
    private readonly JoinGroupCommandHandler _handler;
    private readonly Faker _faker;

    public JoinGroupCommandHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<JoinGroupCommandHandler>>();
        _handler = new JoinGroupCommandHandler(
            _mockRepository.Object,
            _mockEventPublisher.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithPublicGroup_ShouldAddMemberDirectly()
    {
        // Arrange
        var group = GroupFactory.CreateGroup(isPublic: true);
        var profileId = Guid.NewGuid();
        var profileName = _faker.Person.FullName;
        var command = new JoinGroupCommand(group.Id, profileId, profileName);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains(group.Members, m => m.Id == profileId);
        _mockRepository.Verify(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPrivateGroup_ShouldCreateJoinRequest()
    {
        // Arrange
        var group = GroupFactory.CreateGroup(isPublic: false);
        var profileId = Guid.NewGuid();
        var profileName = _faker.Person.FullName;
        var command = new JoinGroupCommand(group.Id, profileId, profileName);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(group.Members, m => m.Id == profileId);
        Assert.Contains(group.JoinRequests, jr => jr.Id == profileId);
        _mockRepository.Verify(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var profileName = _faker.Person.FullName;
        var command = new JoinGroupCommand(groupId, profileId, profileName);

        _mockRepository.Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAlreadyMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var profileName = _faker.Person.FullName;
        var group = GroupFactory.CreateGroup(ownerId: profileId, ownerName: profileName);
        var command = new JoinGroupCommand(group.Id, profileId, profileName);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var profileName = _faker.Person.FullName;
        var command = new JoinGroupCommand(groupId, profileId, profileName);

        _mockRepository.Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
