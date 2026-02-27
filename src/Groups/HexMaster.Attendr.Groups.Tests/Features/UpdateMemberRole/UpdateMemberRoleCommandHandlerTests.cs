using Bogus;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.UpdateMemberRole;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.UpdateMemberRole;

public sealed class UpdateMemberRoleCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<UpdateMemberRoleCommandHandler>> _mockLogger;
    private readonly UpdateMemberRoleCommandHandler _handler;
    private readonly Faker _faker;

    public UpdateMemberRoleCommandHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<UpdateMemberRoleCommandHandler>>();
        _handler = new UpdateMemberRoleCommandHandler(
            _mockRepository.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WhenOwnerUpdatesRole_ShouldUpdateMemberRole()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var memberId = Guid.NewGuid();
        group.AddMember(memberId, _faker.Person.FullName, GroupRole.Member);

        var command = new UpdateMemberRoleCommand(group.Id, memberId, GroupRole.Manager, ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedMember = group.Members.First(m => m.Id == memberId);
        Assert.Equal(GroupRole.Manager, updatedMember.Role);
        _mockRepository.Verify(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowInvalidOperationException()
    {
        var command = new UpdateMemberRoleCommand(Guid.NewGuid(), Guid.NewGuid(), GroupRole.Manager, Guid.NewGuid());
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberNotInGroup_ShouldThrowInvalidOperationException()
    {
        var group = GroupFactory.CreateGroup();
        var command = new UpdateMemberRoleCommand(group.Id, Guid.NewGuid(), GroupRole.Manager, Guid.NewGuid());

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberIsNotOwner_ShouldThrowInvalidOperationException()
    {
        var group = GroupFactory.CreateGroup();
        var regularMemberId = Guid.NewGuid();
        group.AddMember(regularMemberId, _faker.Person.FullName, GroupRole.Member);
        var command = new UpdateMemberRoleCommand(group.Id, Guid.NewGuid(), GroupRole.Manager, regularMemberId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenMemberToUpdateNotFound_ShouldThrowInvalidOperationException()
    {
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var command = new UpdateMemberRoleCommand(group.Id, Guid.NewGuid(), GroupRole.Manager, ownerId);

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
