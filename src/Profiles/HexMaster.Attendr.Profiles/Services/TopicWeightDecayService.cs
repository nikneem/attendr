using HexMaster.Attendr.Profiles.Constants;

namespace HexMaster.Attendr.Profiles.Services;

/// <summary>
/// Service for calculating time-based weight decay for topic occasions.
/// </summary>
public sealed class TopicWeightDecayService
{
    /// <summary>
    /// Calculates the effective weight of an occasion based on its age.
    /// Uses exponential decay where weight loss accelerates over time.
    /// After MaxOccasionAgeMonths, the occasion contributes no weight.
    /// </summary>
    /// <param name="originalWeight">The original weight of the occasion (0-100).</param>
    /// <param name="occasionDate">The date when the occasion was created.</param>
    /// <param name="currentDate">The current date for comparison (defaults to UtcNow).</param>
    /// <returns>The decayed weight value.</returns>
    public int CalculateDecayedWeight(int originalWeight, DateTimeOffset occasionDate, DateTimeOffset? currentDate = null)
    {
        var now = currentDate ?? DateTimeOffset.UtcNow;
        var age = now - occasionDate;
        var ageInMonths = age.TotalDays / 30.0; // Approximate months

        // If older than max age, weight is 0
        if (ageInMonths >= TopicWeightConstants.MaxOccasionAgeMonths)
        {
            return 0;
        }

        // Calculate decay factor using exponential function
        // Formula: weight * e^(-k * t)
        // Where k is chosen so that at t = MaxOccasionAgeMonths, the weight reaches near 0
        // Using a smaller decay rate to ensure weight loss accelerates gradually
        // k = 3 / MaxOccasionAgeMonths gives approximately:
        // - 75% remaining at 6 months
        // - 55% remaining at 12 months  
        // - 30% remaining at 18 months
        // - 17% remaining at 24 months
        // - ~5% remaining at 30 months
        // - <1% remaining at 36 months
        var decayRate = 3.0 / TopicWeightConstants.MaxOccasionAgeMonths;
        var decayFactor = Math.Exp(-decayRate * ageInMonths);

        var decayedWeight = originalWeight * decayFactor;

        return (int)Math.Round(decayedWeight);
    }
}
