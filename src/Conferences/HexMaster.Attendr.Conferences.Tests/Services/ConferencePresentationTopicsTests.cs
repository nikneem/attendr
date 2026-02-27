using HexMaster.Attendr.Conferences.Services;

namespace HexMaster.Attendr.Conferences.Tests.Services;

public class ConferencePresentationTopicsTests
{
    [Fact]
    public void ConferencePresentationTopics_WithTopics_StoresTopicsList()
    {
        var topics = new List<string> { "Azure Functions", "Serverless", "Cloud Architecture" };
        var record = new ConferencePresentationTopics(topics);

        Assert.Equal(topics, record.Topics);
    }

    [Fact]
    public void ConferencePresentationTopics_NullTopics_IsNull()
    {
        var record = new ConferencePresentationTopics(null!);

        Assert.Null(record.Topics);
    }

    [Fact]
    public void ConferencePresentationTopics_EmptyTopics_IsEmpty()
    {
        var record = new ConferencePresentationTopics(new List<string>());

        Assert.Empty(record.Topics);
    }
}
