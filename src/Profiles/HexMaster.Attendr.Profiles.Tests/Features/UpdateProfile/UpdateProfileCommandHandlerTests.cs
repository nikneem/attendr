using Bogus;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Features.UpdateProfile;
using HexMaster.Attendr.Profiles.Observability;
using HexMaster.Attendr.Profiles.Repositories;
using HexMaster.Attendr.Profiles.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.Attendr.Profiles.Tests.Features.UpdateProfile;

public class UpdateProfileCommandHandlerTests
{
    private readonly Mock<IProfileRepository> _repository = new();
    private readonly Mock<IAttendrCacheClient> _cache = new();
    private readonly ProfileMetrics _metrics;
    private readonly Mock<ILogger<UpdateProfileCommandHandler>> _logger = new();
    private readonly UpdateProfileCommandHandler _handler;
    private readonly Faker _faker = new();

    public UpdateProfileCommandHandlerTests()
    {
        _metrics = TestMetricsFactory.CreateProfileMetrics();
        _handler = new UpdateProfileCommandHandler(_repository.Object, _cache.Object, _metrics, _logger.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProfile_WhenFound()
    {
        var subjectId = _faker.Random.Guid().ToString();
        var profile = Profile.FromPersisted(
            _faker.Random.Guid().ToString(),
            subjectId,
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email,
            null,
            null,
            true,
            false);

        _repository.Setup(r => r.GetBySubjectIdAsync(subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var command = new UpdateProfileCommand(
            subjectId,
            "New Display",
            "NewFirst",
            "NewLast",
            "New tagline",
            true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(profile.Id, result.ProfileId);
        Assert.Equal(command.DisplayName, result.DisplayName);
        Assert.Equal(command.FirstName, result.FirstName);
        Assert.Equal(command.LastName, result.LastName);
        Assert.Equal(command.TagLine, result.TagLine);
        Assert.Equal(command.IsSearchable, result.IsSearchable);

        _repository.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.SetAsync(CacheKeys.Profiles.Subject(subjectId), It.IsAny<ResolveProfileResult>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProfileNotFound()
    {
        var subjectId = _faker.Random.Guid().ToString();
        _repository.Setup(r => r.GetBySubjectIdAsync(subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile?)null);

        var command = new UpdateProfileCommand(subjectId, "A", "B", "C", null, false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSubjectIdMissing()
    {
        var command = new UpdateProfileCommand(string.Empty, "A", "B", "C", null, false);
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTaglineTooLong()
    {
        var subjectId = _faker.Random.Guid().ToString();
        var profile = Profile.FromPersisted(
            _faker.Random.Guid().ToString(),
            subjectId,
            _faker.Person.FullName,
            _faker.Person.FirstName,
            _faker.Person.LastName,
            _faker.Person.Email,
            null,
            null,
            true,
            false);

        _repository.Setup(r => r.GetBySubjectIdAsync(subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var longTag = new string('a', 281);
        var command = new UpdateProfileCommand(subjectId, "A", "B", "C", longTag, false);

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
