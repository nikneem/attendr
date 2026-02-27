using HexMaster.Attendr.Conferences.Features.DeleteConference;
using HexMaster.Attendr.Conferences.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.DeleteConference;

public class DeleteConferenceCommandHandlerTests
{
    private readonly Mock<IConferenceRepository> _mockRepository;
    private readonly Mock<ILogger<DeleteConferenceCommandHandler>> _mockLogger;
    private readonly DeleteConferenceCommandHandler _handler;

    public DeleteConferenceCommandHandlerTests()
    {
        _mockRepository = new Mock<IConferenceRepository>();
        _mockLogger = new Mock<ILogger<DeleteConferenceCommandHandler>>();
        var metrics = TestMetricsFactory.CreateConferenceMetrics();

        _handler = new DeleteConferenceCommandHandler(
            _mockRepository.Object,
            metrics,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenConferenceExists_ShouldReturnTrue()
    {
        var id = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteConferenceCommand(id), CancellationToken.None);

        Assert.True(result);
        _mockRepository.Verify(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConferenceNotFound_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(new DeleteConferenceCommand(id), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagate()
    {
        var id = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new DeleteConferenceCommand(id), CancellationToken.None));
    }
}
