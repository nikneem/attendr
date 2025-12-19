namespace HexMaster.Attendr.Groups.DomainModels;

public sealed class GroupSettings
{
    public bool IsPublic { get; set; }
    public bool IsSearchable { get; set; }

    public GroupSettings()
    {
        IsPublic = false;
        IsSearchable = false;
    }

    public GroupSettings(bool isPublic, bool isSearchable)
    {
        IsPublic = isPublic;
        IsSearchable = isSearchable;
    }
}
