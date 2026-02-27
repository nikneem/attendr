using Bogus;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.ApproveJoinRequest;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.ApproveJoinRequest;

public sealed class ApproveJoinRequestCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<ApproveJoinRequestCommandHandler>> _mockLogger;
    private readonly ApproveJoinRequestCommandHandler _handler;
    private readonly Faker _faker;

    public ApproveJoinRequestCommandHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<ApproveJoinRequestCommandHandler>>();
        _handler = new ApproveJoinRequestCommandHandler(
            _mockRepository.Object,
            _mockEventPublisher.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithValidOwnerApproval_ShouldApproveMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var requesterId = Guid.NewGuid();
        group.AddJoinRequest(requesterId, _faker.Person.FullName);

        var command = new ApproveJoinRequestCommand(group.Id, requesterId, ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains(group.Members, m => m.Id == requesterId);
        Assert.Empty(group.JoinRequests);
        _mockRepository.Verify(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidManagerApproval_ShouldApproveMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var managerId = Guid.NewGuid();
        group.AddMember(managerId, _faker.Person.FullName, GroupRole.Manager);
        var requesterId = Guid.NewGuid();
        group.AddJoinRequest(requesterId, _faker.Person.FullName);

        var command = new ApproveJoinRequestCommand(group.Id, requesterId, managerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains(group.Members, m => m.Id == requesterId);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new ApproveJoinRequestCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberNotInGroup_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = GroupFactory.CreateGroup();
        var command = new ApproveJoinRequestCommand(group.Id, Guid.NewGuid(), Guid.NewGuid());

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberIsRegularMember_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var group = GroupFactory.CreateGroup();
        var regularMemberId = Guid.NewGuid();
        group.AddMember(regularMemberId, _faker.Person.FullName, GroupRole.Member);
        var command = new ApproveJoinRequestCommand(group.Id, Guid.NewGuid(), regularMemberId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        // Act & Assert
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
