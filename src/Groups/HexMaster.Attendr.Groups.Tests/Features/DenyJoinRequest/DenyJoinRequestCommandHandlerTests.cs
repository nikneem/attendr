using Bogus;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;
using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Features.DenyJoinRequest;
using HexMaster.Attendr.Groups.Observability;
using HexMaster.Attendr.Groups.Repositories;
using HexMaster.Attendr.Groups.Tests.Factories;
using HexMaster.Attendr.Groups.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Features.DenyJoinRequest;

public sealed class DenyJoinRequestCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _mockRepository;
    private readonly GroupMetrics _metrics;
    private readonly Mock<ILogger<DenyJoinRequestCommandHandler>> _mockLogger;
    private readonly DenyJoinRequestCommandHandler _handler;
    private readonly Faker _faker;

    public DenyJoinRequestCommandHandlerTests()
    {
        _mockRepository = new Mock<IGroupRepository>();
        _metrics = TestMetricsFactory.CreateGroupMetrics();
        _mockLogger = new Mock<ILogger<DenyJoinRequestCommandHandler>>();
        _handler = new DenyJoinRequestCommandHandler(
            _mockRepository.Object,
            _metrics,
            _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_WithValidOwnerDenial_ShouldRemoveJoinRequest()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var group = GroupFactory.CreateGroup(ownerId: ownerId, ownerName: _faker.Person.FullName);
        var requesterId = Guid.NewGuid();
        group.AddJoinRequest(requesterId, _faker.Person.FullName);

        var command = new DenyJoinRequestCommand(group.Id, requesterId, ownerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Empty(group.JoinRequests);
        Assert.DoesNotContain(group.Members, m => m.Id == requesterId);
        _mockRepository.Verify(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidManagerDenial_ShouldRemoveJoinRequest()
    {
        // Arrange
        var group = GroupFactory.CreateGroup();
        var managerId = Guid.NewGuid();
        group.AddMember(managerId, _faker.Person.FullName, GroupRole.Manager);
        var requesterId = Guid.NewGuid();
        group.AddJoinRequest(requesterId, _faker.Person.FullName);

        var command = new DenyJoinRequestCommand(group.Id, requesterId, managerId);

        _mockRepository.Setup(x => x.GetByIdAsync(group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _mockRepository.Setup(x => x.UpdateAsync(group, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Empty(group.JoinRequests);
    }

    [Fact]
    public async Task Handle_WhenGroupNotFound_ShouldThrowInvalidOperationException()
    {
        var command = new DenyJoinRequestCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestingMemberNotInGroup_ShouldThrowInvalidOperationException()
    {
        var group = GroupFactory.CreateGroup();
        var command = new DenyJoinRequestCommand(group.Id, Guid.NewGuid(), Guid.NewGuid());

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
        var command = new DenyJoinRequestCommand(group.Id, Guid.NewGuid(), regularMemberId);

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
