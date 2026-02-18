using HexMaster.Attendr.Core.Constants;

namespace HexMaster.Attendr.Core.Tests.Constants;

public sealed class PaginationConstantsTests
{
    [Fact]
    public void DefaultPageSize_Is25()
    {
        Assert.Equal(25, PaginationConstants.DefaultPageSize);
    }

    [Fact]
    public void MaxPageSize_Is100()
    {
        Assert.Equal(100, PaginationConstants.MaxPageSize);
    }

    [Fact]
    public void MinPageSize_Is1()
    {
        Assert.Equal(1, PaginationConstants.MinPageSize);
    }

    [Fact]
    public void NormalizePageSize_WithNull_ReturnsDefault()
    {
        var result = PaginationConstants.NormalizePageSize(null);
        Assert.Equal(PaginationConstants.DefaultPageSize, result);
    }

    [Fact]
    public void NormalizePageSize_WithValidValue_ReturnsSameValue()
    {
        var result = PaginationConstants.NormalizePageSize(50);
        Assert.Equal(50, result);
    }

    [Fact]
    public void NormalizePageSize_WithZero_ReturnsMin()
    {
        var result = PaginationConstants.NormalizePageSize(0);
        Assert.Equal(PaginationConstants.MinPageSize, result);
    }

    [Fact]
    public void NormalizePageSize_WithNegative_ReturnsMin()
    {
        var result = PaginationConstants.NormalizePageSize(-5);
        Assert.Equal(PaginationConstants.MinPageSize, result);
    }

    [Fact]
    public void NormalizePageSize_AboveMax_ReturnsMax()
    {
        var result = PaginationConstants.NormalizePageSize(9999);
        Assert.Equal(PaginationConstants.MaxPageSize, result);
    }

    [Fact]
    public void NormalizePageSize_WithMin_ReturnsMin()
    {
        var result = PaginationConstants.NormalizePageSize(PaginationConstants.MinPageSize);
        Assert.Equal(PaginationConstants.MinPageSize, result);
    }

    [Fact]
    public void NormalizePageSize_WithMax_ReturnsMax()
    {
        var result = PaginationConstants.NormalizePageSize(PaginationConstants.MaxPageSize);
        Assert.Equal(PaginationConstants.MaxPageSize, result);
    }
}
