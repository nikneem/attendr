using Bogus;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.RemoveMember;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.RemoveMember;

public sealed class RemoveMemberCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly Mock<IIntegrationEventPublisher> _mockEventPublisher;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<RemoveMemberCommandHandler>> _mockLogger;
    private readonly RemoveMemberCommandHandler _handler;
    private readonly Faker _faker;

    public RemoveMemberCommandHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _mockEventPublisher = new Mock<IIntegrationEventPublisher>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<RemoveMemberCommandHandler>>();
        _handler = new RemoveMemberCommandHandler(
            _mockRepository.Object,
            _mockEventPublisher.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WhenOwnerRemovesMember_ShouldRemoveMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        group.AddMember(memberId, _faker.Person.FullName, GroupRole.Member);

        var command = new RemoveMemberCommand(group.Id, memberId, ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(group.Members, m => m.Id == memberId);
        _mockRepository.Verify(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenManagerRemovesMember_ShouldRemoveMember()
    {
        // Arrange
        var group = GroupFactory.CreateGroup();
        var managerId = Guid.NewGuid();
        group.AddMember(managerId, _faker.Person.FullName, GroupRole.Manager);
        var memberId = Guid.NewGuid();
        group.AddMember(memberId, _faker.Person.FullName, GroupRole.Member);

        var command = new RemoveMemberCommand(group.Id, memberId, managerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(group.Members, m => m.Id == memberId);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowInvalidOperationException()
    {
        var command = new RemoveMemberCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberNotInGroup_ShouldThrowInvalidOperationException()
    {
        var group = GroupFactory.CreateGroup();
        var command = new RemoveMemberCommand(group.Id, Guid.NewGuid(), Guid.NewGuid());

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberIsRegularMember_ShouldThrowInvalidOperationException()
    {
        var group = GroupFactory.CreateGroup();
        var regularMemberId = Guid.NewGuid();
        group.AddMember(regularMemberId, _faker.Person.FullName, GroupRole.Member);
        var command = new RemoveMemberCommand(group.Id, Guid.NewGuid(), regularMemberId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenMemberToRemoveNotFound_ShouldThrowInvalidOperationException()
    {
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var command = new RemoveMemberCommand(group.Id, Guid.NewGuid(), ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRemovingLastOwner_ShouldThrowInvalidOperationException()
    {
        // Arrange – only one owner, trying to remove themselves as owner
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var command = new RemoveMemberCommand(group.Id, ownerId, ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

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
