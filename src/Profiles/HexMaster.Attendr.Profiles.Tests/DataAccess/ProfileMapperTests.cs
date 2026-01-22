using Bogus;
using HexMaster.Attendr.Profiles.Data.TableStorage.Mappers;
using HexMaster.Attendr.Profiles.Data.TableStorage.Models;
using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Tests.DataAccess;

public class ProfileMapperTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void ToEntity_ShouldMapDomainToTableEntity()
    {
        var profile = Profile.Create(
            _faker.Random.Guid().ToString(),
            _faker.Name.FullName(),
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            _faker.Internet.Email());

        profile.SetEmployee(_faker.Random.AlphaNumeric(8));
        profile.SetTagLine("Ready to connect");
        profile.SetIsSearchable(true);

        var entity = ProfileMapper.ToEntity(profile);

        Assert.Equal(profile.SubjectId, entity.PartitionKey);
        Assert.Equal(profile.Id, entity.RowKey);
        Assert.Equal(profile.DisplayName, entity.DisplayName);
        Assert.Equal(profile.FirstName, entity.FirstName);
        Assert.Equal(profile.LastName, entity.LastName);
        Assert.Equal(profile.Email, entity.Email);
        Assert.Equal(profile.Employee, entity.Employee);
        Assert.Equal(profile.TagLine, entity.TagLine);
        Assert.Equal(profile.IsSearchable, entity.IsSearchable);
        Assert.Equal(profile.Enabled, entity.Enabled);
    }

    [Fact]
    public void ToDomain_ShouldHandleNullNamesAndNormalizeEmail()
    {
        var id = _faker.Random.Guid().ToString();
        var subjectId = _faker.Random.Guid().ToString();

        var entity = new ProfileEntity
        {
            Id = id,
            SubjectId = subjectId,
            PartitionKey = subjectId,
            RowKey = id,
            DisplayName = "  Jane Doe  ",
            FirstName = null,
            LastName = null,
            Email = "USER@EXAMPLE.COM",
            Employee = null,
            TagLine = null,
            Enabled = false,
            IsSearchable = true
        };

        var domain = ProfileMapper.ToDomain(entity);

        Assert.Equal(id, domain.Id);
        Assert.Equal(subjectId, domain.SubjectId);
        Assert.Equal("Jane Doe", domain.DisplayName);
        Assert.Null(domain.FirstName);
        Assert.Null(domain.LastName);
        Assert.Equal("user@example.com", domain.Email);
        Assert.Null(domain.TagLine);
        Assert.True(domain.IsSearchable);
        Assert.False(domain.Enabled);
    }
}
