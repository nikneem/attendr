using Bogus;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Features.CreateProfile;
using HexMaster.Attendr.Profiles.Repositories;
using HexMaster.Attendr.Profiles.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Profiles.Tests.Features.CreateProfile;

public class CreateProfileCommandHandlerErrorTests
{
    private readonly Mock<IProfileRepository> _mockRepository;
    private readonly Mock<IAttendrCacheClient> _mockCache;
    private readonly Faker _faker;

    public CreateProfileCommandHandlerErrorTests()
    {
        _mockRepository = new Mock<IProfileRepository>();
        _mockCache = new Mock<IAttendrCacheClient>();
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_ShouldThrowAndRecordMetrics_WhenRepositoryGetThrows()
    {
        var metrics = TestMetricsFactory.CreateProfileMetrics();
        var mockLogger = new Mock<ILogger<CreateProfileCommandHandler>>();
        var handler = new CreateProfileCommandHandler(_mockRepository.Object, _mockCache.Object, metrics, mockLogger.Object);

        var command = new CreateProfileCommand(
            _faker.Random.Guid().ToString(),
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        );

        _mockRepository
            .Setup(r => r.GetBySubjectIdAsync(command.SubjectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        _mockRepository.Verify(
            r => r.GetBySubjectIdAsync(command.SubjectId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowAndRecordMetrics_WhenRepositoryAddThrows()
    {
        var metrics = TestMetricsFactory.CreateProfileMetrics();
        var mockLogger = new Mock<ILogger<CreateProfileCommandHandler>>();
        var handler = new CreateProfileCommandHandler(_mockRepository.Object, _mockCache.Object, metrics, mockLogger.Object);

        var command = new CreateProfileCommand(
            _faker.Random.Guid().ToString(),
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        );

        _mockRepository
            .Setup(r => r.GetBySubjectIdAsync(command.SubjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database insert error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        _mockRepository.Verify(
            r => r.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCompleteSuccessfully_WhenCacheSetFails()
    {
        var metrics = TestMetricsFactory.CreateProfileMetrics();
        var mockLogger = new Mock<ILogger<CreateProfileCommandHandler>>();
        var handler = new CreateProfileCommandHandler(_mockRepository.Object, _mockCache.Object, metrics, mockLogger.Object);

        var command = new CreateProfileCommand(
            _faker.Random.Guid().ToString(),
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        );

        _mockRepository
            .Setup(r => r.GetBySubjectIdAsync(command.SubjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCache
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<ResolveProfileResult>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cache error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        var metrics = TestMetricsFactory.CreateProfileMetrics();
        var mockLogger = new Mock<ILogger<CreateProfileCommandHandler>>();
        var handler = new CreateProfileCommandHandler(_mockRepository.Object, _mockCache.Object, metrics, mockLogger.Object);

        var command = new CreateProfileCommand(
            _faker.Random.Guid().ToString(),
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        );

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepository
            .Setup(r => r.GetBySubjectIdAsync(command.SubjectId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() => handler.Handle(command, cts.Token));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenMetricsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CreateProfileCommandHandler(
            new Mock<IProfileRepository>().Object,
            new Mock<IAttendrCacheClient>().Object,
            null!,
            new Mock<ILogger<CreateProfileCommandHandler>>().Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var metrics = TestMetricsFactory.CreateProfileMetrics();

        Assert.Throws<ArgumentNullException>(() => new CreateProfileCommandHandler(
            new Mock<IProfileRepository>().Object,
            new Mock<IAttendrCacheClient>().Object,
            metrics,
            null!));
    }
}
