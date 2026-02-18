using HexMaster.Attendr.IntegrationEvents.Models;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Models;

public class PresentationTopicDtoTests
{
    [Fact]
    public void PresentationTopicDto_Constructor_SetsProperties()
    {
        var dto = new PresentationTopicDto("csharp", "C#");

        Assert.Equal("csharp", dto.Key);
        Assert.Equal("C#", dto.Name);
    }

    [Fact]
    public void PresentationTopicDto_RecordEquality_WhenSameValues()
    {
        var dto1 = new PresentationTopicDto("azure", "Azure");
        var dto2 = new PresentationTopicDto("azure", "Azure");

        Assert.Equal(dto1, dto2);
    }

    [Fact]
    public void PresentationTopicDto_RecordInequality_WhenDifferentKey()
    {
        var dto1 = new PresentationTopicDto("azure", "Azure");
        var dto2 = new PresentationTopicDto("aws", "Azure");

        Assert.NotEqual(dto1, dto2);
    }

    [Fact]
    public void PresentationTopicDto_RecordInequality_WhenDifferentName()
    {
        var dto1 = new PresentationTopicDto("cloud", "Azure");
        var dto2 = new PresentationTopicDto("cloud", "AWS");

        Assert.NotEqual(dto1, dto2);
    }

    [Fact]
    public void PresentationTopicDto_HashCode_EqualForSameValues()
    {
        var dto1 = new PresentationTopicDto("dotnet", ".NET");
        var dto2 = new PresentationTopicDto("dotnet", ".NET");

        Assert.Equal(dto1.GetHashCode(), dto2.GetHashCode());
    }

    [Fact]
    public void PresentationTopicDto_ToString_ContainsKeyAndName()
    {
        var dto = new PresentationTopicDto("kubernetes", "Kubernetes");
        var str = dto.ToString();

        Assert.Contains("kubernetes", str);
        Assert.Contains("Kubernetes", str);
    }
}
