using Dapr.Client;
using HexMaster.Attendr.Core.Cache;
using Moq;

namespace HexMaster.Attendr.Core.Tests.Cache;

public sealed class AttendrCacheClientTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly AttendrCacheClient _sut;

    public AttendrCacheClientTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _sut = new AttendrCacheClient(_daprClientMock.Object);
    }

    // ──────────────────────────── GetOrSetAsync ────────────────────────────

    [Fact]
    public async Task GetOrSetAsync_WithNullOrWhitespaceKey_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetOrSetAsync<string>("", ct => Task.FromResult<string?>(null)));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetOrSetAsync<string>(" ", ct => Task.FromResult<string?>(null)));
    }

    [Fact]
    public async Task GetOrSetAsync_WithNullFactory_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.GetOrSetAsync<string>("key", null!));
    }

    [Fact]
    public async Task GetOrSetAsync_WhenCacheHit_ReturnsCachedValue_WithoutCallingFactory()
    {
        const string cachedValue = "cached";
        _daprClientMock
            .Setup(d => d.GetStateAsync<string>(It.IsAny<string>(), "my-key",
                It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedValue);

        var factoryCalled = false;
        var result = await _sut.GetOrSetAsync<string>("my-key", ct =>
        {
            factoryCalled = true;
            return Task.FromResult<string?>("fresh");
        });

        Assert.Equal(cachedValue, result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task GetOrSetAsync_WhenCacheMiss_CallsFactoryAndSavesResult()
    {
        // Cache returns null (miss)
        _daprClientMock
            .Setup(d => d.GetStateAsync<string>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _daprClientMock
            .Setup(d => d.SaveStateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<StateOptions?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.GetOrSetAsync<string>("key", ct => Task.FromResult<string?>("fresh"));

        Assert.Equal("fresh", result);
        _daprClientMock.Verify(d => d.SaveStateAsync(It.IsAny<string>(), "key", "fresh",
            It.IsAny<StateOptions?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrSetAsync_WhenFactoryReturnsNull_DoesNotSaveAndReturnsNull()
    {
        _daprClientMock
            .Setup(d => d.GetStateAsync<string>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _sut.GetOrSetAsync<string>("key", ct => Task.FromResult<string?>(null));

        Assert.Null(result);
        _daprClientMock.Verify(d => d.SaveStateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<StateOptions?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrSetAsync_WhenCacheRetrievalThrows_FallsThroughToFactory()
    {
        _daprClientMock
            .Setup(d => d.GetStateAsync<string>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("dapr down"));

        _daprClientMock
            .Setup(d => d.SaveStateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<StateOptions?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.GetOrSetAsync<string>("key", ct => Task.FromResult<string?>("fallback"));

        Assert.Equal("fallback", result);
    }

    [Fact]
    public async Task GetOrSetAsync_WithCustomTtl_UsesTtlInMetadata()
    {
        _daprClientMock
            .Setup(d => d.GetStateAsync<string>(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        Dictionary<string, string>? capturedMetadata = null;
        _daprClientMock
            .Setup(d => d.SaveStateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<StateOptions?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, string, StateOptions?, IReadOnlyDictionary<string, string>?, CancellationToken>(
                (_, _, _, _, meta, _) => capturedMetadata = meta?.ToDictionary(k => k.Key, v => v.Value))
            .Returns(Task.CompletedTask);

        await _sut.GetOrSetAsync<string>("key", ct => Task.FromResult<string?>("val"), TimeSpan.FromSeconds(30));

        Assert.NotNull(capturedMetadata);
        Assert.Equal("30", capturedMetadata!["ttlInSeconds"]);
    }

    // ──────────────────────────── SetAsync ────────────────────────────

    [Fact]
    public async Task SetAsync_WithNullOrWhitespaceKey_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.SetAsync("", "val"));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.SetAsync("   ", "val"));
    }

    [Fact]
    public async Task SetAsync_CallsSaveStateAsync()
    {
        _daprClientMock
            .Setup(d => d.SaveStateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<StateOptions?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.SetAsync("my-key", "my-value");

        _daprClientMock.Verify(d => d.SaveStateAsync(It.IsAny<string>(), "my-key", "my-value",
            It.IsAny<StateOptions?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithCustomTtl_IncludesTtlInMetadata()
    {
        Dictionary<string, string>? capturedMetadata = null;
        _daprClientMock
            .Setup(d => d.SaveStateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<StateOptions?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, string, StateOptions?, IReadOnlyDictionary<string, string>?, CancellationToken>(
                (_, _, _, _, meta, _) => capturedMetadata = meta?.ToDictionary(k => k.Key, v => v.Value))
            .Returns(Task.CompletedTask);

        await _sut.SetAsync("key", "value", TimeSpan.FromSeconds(60));

        Assert.NotNull(capturedMetadata);
        Assert.Equal("60", capturedMetadata!["ttlInSeconds"]);
    }
}
