using Bogus;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.CreateProfile;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Profiles.Tests.CreateProfile;

public class CreateProfileCommandHandlerTests
{
    private readonly Mock<IProfileRepository> _mockRepository;
    private readonly Mock<IAttendrCacheClient> _mockCache;
    private readonly Mock<ILogger<CreateProfileCommandHandler>> _mockLogger;
    private readonly CreateProfileCommandHandler _handler;
    private readonly Faker _faker;

    public CreateProfileCommandHandlerTests()
    {
        _mockRepository = new Mock<IProfileRepository>();
        _mockCache = new Mock<IAttendrCacheClient>();
        _mockLogger = new Mock<ILogger<CreateProfileCommandHandler>>();
        _handler = new CreateProfileCommandHandler(_mockRepository.Object, _mockCache.Object, _mockLogger.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task Handle_ShouldCreateNewProfile_WhenProfileDoesNotExist()
    {
        // Arrange
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ProfileId);
        Assert.NotEmpty(result.ProfileId);

        _mockRepository.Verify(
            r => r.GetBySubjectIdAsync(command.SubjectId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRepository.Verify(
            r => r.AddAsync(It.Is<Profile>(p =>
                p.SubjectId == command.SubjectId &&
                p.DisplayName == command.DisplayName &&
                p.FirstName == command.FirstName &&
                p.LastName == command.LastName &&
                p.Email == command.Email.ToLowerInvariant()
            ), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify cache priming on newly created profile
        _mockCache.Verify(
            c => c.SetAsync(
                It.Is<string>(k => k == CacheKeys.Profiles.Subject(command.SubjectId)),
                It.Is<ResolveProfileResult>(res => !string.IsNullOrEmpty(res.ProfileId) && res.DisplayName == command.DisplayName),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingProfileId_WhenProfileAlreadyExists()
    {
        // Arrange
        var existingProfileId = _faker.Random.Guid().ToString();
        var subjectId = _faker.Random.Guid().ToString();

        var command = new CreateProfileCommand(
            subjectId,
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        );

        var existingProfile = Profile.FromPersisted(
            existingProfileId,
            subjectId,
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email,
            null,
            null,
            true,
            false
        );

        _mockRepository
            .Setup(r => r.GetBySubjectIdAsync(command.SubjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProfile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingProfileId, result.ProfileId);

        _mockRepository.Verify(
            r => r.GetBySubjectIdAsync(command.SubjectId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRepository.Verify(
            r => r.AddAsync(It.IsAny<Profile>(), It.IsAny<CancellationToken>()),
            Times.Never);

        // Verify cache priming on existing profile
        _mockCache.Verify(
            c => c.SetAsync(
                It.Is<string>(k => k == CacheKeys.Profiles.Subject(command.SubjectId)),
                It.Is<ResolveProfileResult>(res => res.ProfileId == existingProfileId && !string.IsNullOrEmpty(res.DisplayName)),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenCommandIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenSubjectIdIsEmpty()
    {
        // Arrange
        var command = new CreateProfileCommand(
            string.Empty,
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenSubjectIdIsWhitespace()
    {
        // Arrange
        var command = new CreateProfileCommand(
            "   ",
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateProfileCommandHandler(null!, new Mock<IAttendrCacheClient>().Object, new Mock<ILogger<CreateProfileCommandHandler>>().Object));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCacheIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateProfileCommandHandler(new Mock<IProfileRepository>().Object, null!, new Mock<ILogger<CreateProfileCommandHandler>>().Object));
    }
}
