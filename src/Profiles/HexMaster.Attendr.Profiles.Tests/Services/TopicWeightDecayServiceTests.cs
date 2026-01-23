using HexMaster.Attendr.Profiles.Constants;
using HexMaster.Attendr.Profiles.Services;

namespace HexMaster.Attendr.Profiles.Tests.Services;

public class TopicWeightDecayServiceTests
{
    private readonly TopicWeightDecayService _service = new();

    [Fact]
    public void CalculateDecayedWeight_ShouldReturnFullWeight_ForRecentOccasion()
    {
        // Arrange
        var originalWeight = 100;
        var occasionDate = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        var result = _service.CalculateDecayedWeight(originalWeight, occasionDate);

        // Assert - Should be close to original weight (within 5% for 1 day old)
        Assert.True(result >= 95, $"Expected weight >= 95, but got {result}");
    }

    [Fact]
    public void CalculateDecayedWeight_ShouldReduceWeight_ForOlderOccasion()
    {
        // Arrange
        var originalWeight = 100;
        var occasionDate = DateTimeOffset.UtcNow.AddMonths(-18); // Half the max age

        // Act
        var result = _service.CalculateDecayedWeight(originalWeight, occasionDate);

        // Assert - Should have significant decay but not be zero
        Assert.True(result > 0 && result < 100, $"Expected weight between 0 and 100, but got {result}");
        Assert.True(result < 50, $"Expected weight < 50 for half-life, but got {result}");
    }

    [Fact]
    public void CalculateDecayedWeight_ShouldReturnZero_ForVeryOldOccasion()
    {
        // Arrange
        var originalWeight = 100;
        var occasionDate = DateTimeOffset.UtcNow.AddMonths(-TopicWeightConstants.MaxOccasionAgeMonths);

        // Act
        var result = _service.CalculateDecayedWeight(originalWeight, occasionDate);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateDecayedWeight_ShouldReturnZero_ForOccasionBeyondMaxAge()
    {
        // Arrange
        var originalWeight = 100;
        var occasionDate = DateTimeOffset.UtcNow.AddMonths(-(TopicWeightConstants.MaxOccasionAgeMonths + 6));

        // Act
        var result = _service.CalculateDecayedWeight(originalWeight, occasionDate);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateDecayedWeight_ShouldShowExponentialDecay()
    {
        // Arrange
        var originalWeight = 100;
        var now = DateTimeOffset.UtcNow;

        // Act - Calculate weights at different ages
        var weight6Months = _service.CalculateDecayedWeight(originalWeight, now.AddMonths(-6), now);
        var weight12Months = _service.CalculateDecayedWeight(originalWeight, now.AddMonths(-12), now);
        var weight24Months = _service.CalculateDecayedWeight(originalWeight, now.AddMonths(-24), now);
        var weight30Months = _service.CalculateDecayedWeight(originalWeight, now.AddMonths(-30), now);

        // Assert - Exponential decay means the weight decreases continuously
        Assert.True(weight6Months > weight12Months,
            $"Expected weight6Months ({weight6Months}) > weight12Months ({weight12Months})");
        Assert.True(weight12Months > weight24Months,
            $"Expected weight12Months ({weight12Months}) > weight24Months ({weight24Months})");
        Assert.True(weight24Months > weight30Months,
            $"Expected weight24Months ({weight24Months}) > weight30Months ({weight30Months})");

        // Verify exponential nature: weight should be substantially reduced over time
        Assert.True(weight6Months > 40, $"Expected significant weight remaining after 6 months, got {weight6Months}");
        Assert.True(weight24Months < 25, $"Expected less than 25% remaining after 24 months, got {weight24Months}");
        Assert.True(weight30Months < 10, $"Expected less than 10% remaining after 30 months, got {weight30Months}");
    }

    [Fact]
    public void CalculateDecayedWeight_ShouldHandleZeroWeight()
    {
        // Arrange
        var originalWeight = 0;
        var occasionDate = DateTimeOffset.UtcNow.AddMonths(-6);

        // Act
        var result = _service.CalculateDecayedWeight(originalWeight, occasionDate);

        // Assert
        Assert.Equal(0, result);
    }
}
