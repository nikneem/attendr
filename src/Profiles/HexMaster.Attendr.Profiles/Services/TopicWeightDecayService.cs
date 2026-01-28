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

    /// <summary>
    /// Calculates the total weight for a topic by summing all decayed occasion weights.
    /// Manual topics always return 100. Non-manual topics sum all occasion weights with decay applied,
    /// filtered by maximum age, and capped at 100.
    /// </summary>
    /// <param name="isManual">Whether the topic is manually confirmed.</param>
    /// <param name="occasions">The collection of occasions for the topic.</param>
    /// <param name="currentDate">The current date for comparison (defaults to UtcNow).</param>
    /// <returns>The total topic weight (0-100).</returns>
    public int CalculateTopicWeight(
        bool isManual,
        IEnumerable<(int Weight, DateTimeOffset Date)> occasions,
        DateTimeOffset? currentDate = null)
    {
        // Manual topics always have a weight of 100
        if (isManual)
        {
            return 100;
        }

        var now = currentDate ?? DateTimeOffset.UtcNow;
        var maxAgeDate = now.AddMonths(-TopicWeightConstants.MaxOccasionAgeMonths);

        // Filter occasions within the max age timespan and calculate total decayed weight
        var totalDecayedWeight = occasions
            .Where(o => o.Date >= maxAgeDate)
            .Sum(o => CalculateDecayedWeight(o.Weight, o.Date, now));

        // Cap the total weight at 100
        return Math.Min(totalDecayedWeight, 100);
    }
}
