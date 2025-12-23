namespace HexMaster.Attendr.Core.Configuration;

public sealed class DaprOptions
{
    public const string SectionName = "Dapr";

    public string SharedStateStoreName { get; set; } = "statestore";
    public string PubSubName { get; set; } = "pubsub";
}
