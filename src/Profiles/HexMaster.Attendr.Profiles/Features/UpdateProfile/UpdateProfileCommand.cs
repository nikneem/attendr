using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Profiles.Features.UpdateProfile;

public sealed record UpdateProfileCommand(
    string SubjectId,
    string DisplayName,
    string FirstName,
    string LastName,
    string? TagLine,
    bool IsSearchable) : IAttendrCommand;
