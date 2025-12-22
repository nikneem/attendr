using HexMaster.Attendr.Profiles.Data.MongoDb.Models;
using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.Data.MongoDb.Mappers;

/// <summary>
/// Maps between Profile domain model and ProfileDocument.
/// </summary>
internal static class ProfileMapper
{
    public static ProfileDocument ToDocument(Profile profile)
    {
        return new ProfileDocument
        {
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

    public static Profile ToDomain(ProfileDocument document)
    {
        return Profile.FromPersisted(
            document.Id,
            document.SubjectId,
            document.DisplayName,
            document.FirstName ?? string.Empty,
            document.LastName ?? string.Empty,
            document.Email,
            document.Employee,
            document.TagLine,
            document.Enabled,
            document.IsSearchable);
    }
}
