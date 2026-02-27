using HexMaster.Attendr.Conferences.Services;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Sessionize.Api.Client.Abstractions;

namespace HexMaster.Attendr.Conferences.Tests.Services;

public class SessionizeSyncServiceTests
{
    private readonly Mock<IConferenceRepository> _conferenceRepositoryMock = new();
    private readonly Mock<ISessionizeApiClient> _sessionizeApiClientMock = new();
    private readonly Mock<IIntegrationEventPublisher> _eventPublisherMock = new();
    private readonly Mock<ILogger<SessionizeSyncService>> _loggerMock = new();

    private SessionizeSyncService CreateService() =>
        new SessionizeSyncService(
            _conferenceRepositoryMock.Object,
            _sessionizeApiClientMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);

    [Fact]
    public void Constructor_ValidArgs_CreatesInstance()
    {
        var service = CreateService();
        Assert.NotNull(service);
    }

    [Fact]
    public async Task SynchronizeConferenceAsync_ConferenceNotFound_ReturnsNull()
    {
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HexMaster.Attendr.Conferences.DomainModels.Conference?)null);

        var service = CreateService();
        var result = await service.SynchronizeConferenceAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SynchronizeConferenceAsync_NoSyncSource_ReturnsResultWithCounts()
    {
        var conference = ConferenceFactory.CreatePersistedConference(synchronizationSource: null);
        _conferenceRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        var service = CreateService();
        var result = await service.SynchronizeConferenceAsync(conference.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(conference.Id, result.ConferenceId);
        Assert.Equal(conference.Speakers.Count, result.SpeakersCount);
        Assert.Equal(conference.Rooms.Count, result.RoomsCount);
        Assert.Equal(conference.Presentations.Count, result.PresentationsCount);
    }
}
