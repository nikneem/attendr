using HexMaster.Attendr.Profiles.Data.TableStorage.Models;
using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Data.TableStorage.Mappers;

/// <summary>
/// Maps between Profile domain model and ProfileEntity.
/// </summary>
internal static class ProfileMapper
{
    public static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity
        {
            PartitionKey = profile.SubjectId,
            RowKey = profile.Id,
            Id = profile.Id,
            SubjectId = profile.SubjectId,
            DisplayName = profile.DisplayName,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            Employee = profile.Employee,
            TagLine = profile.TagLine,
            IsSearchable = profile.IsSearchable,
            Enabled = profile.Enabled
        };
    }

    public static Profile ToDomain(ProfileEntity entity)
    {
        return Profile.FromPersisted(
            entity.Id,
            entity.SubjectId,
            entity.DisplayName,
            entity.FirstName ?? string.Empty,
            entity.LastName ?? string.Empty,
            entity.Email,
            entity.Employee,
            entity.TagLine,
            entity.Enabled,
            entity.IsSearchable);
    }
}
