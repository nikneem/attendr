namespace HexMaster.Attendr.Profiles.Constants;

/// <summary>
/// Constants related to topic weight calculations and decay.
/// </summary>
public static class TopicWeightConstants
{
    /// <summary>
    /// The timespan in months after which an occasion contributes no weight.
    /// Set to 36 months (3 years).
    /// </summary>
    public const int MaxOccasionAgeMonths = 36;
}
