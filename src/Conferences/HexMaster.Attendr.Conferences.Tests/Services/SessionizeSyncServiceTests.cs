using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Services;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Sessionize.Api.Client.Abstractions;
using Sessionize.Api.Client.DataTransferObjects;

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
            .ReturnsAsync((Conference?)null);

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

    [Fact]
    public async Task SynchronizeConferenceAsync_WithSessionizeSyncAndEmptyData_SyncsSuccessfully()
    {
        // Conference with Sessionize sync source
        var syncSource = SynchronizationSource.CreateWithUrl(SynchronizationSourceType.Sessionize, "my-sessionize-api-key");
        var conference = ConferenceFactory.CreatePersistedConference(synchronizationSource: syncSource);

        _conferenceRepositoryMock
            .Setup(r => r.GetByIdAsync(conference.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conference);

        _conferenceRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Sessionize returns no speakers or schedule
        _sessionizeApiClientMock
            .Setup(c => c.GetSpeakersListAsync(It.IsAny<string?>(), It.IsAny<CancellationToken?>()))
            .ReturnsAsync(new List<SpeakerDetailsResponse>());

        _sessionizeApiClientMock
            .Setup(c => c.GetScheduleGridAsync(It.IsAny<string?>(), It.IsAny<CancellationToken?>()))
            .ReturnsAsync(new List<ScheduleGridResponse>());

        var service = CreateService();
        var result = await service.SynchronizeConferenceAsync(conference.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(conference.Id, result.ConferenceId);
        _conferenceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Conference>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
