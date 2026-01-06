using HexMaster.Attendr.Core.Constants;

namespace HexMaster.Attendr.Core.Configuration;

public sealed class DaprOptions
{
    public const string SectionName = "Dapr";

    public string SharedStateStoreName { get; set; } = DaprConstants.StateStore.SharedStateStoreName;
    public string PubSubName { get; set; } = DaprConstants.PubSub.Name;
}
