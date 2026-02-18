using HexMaster.Attendr.Core.Exceptions;

namespace HexMaster.Attendr.Core.Tests.Exceptions;

public sealed class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new DomainException("something went wrong");
        Assert.Equal("something went wrong", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new DomainException("outer", inner);
        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void DomainException_IsException()
    {
        var ex = new DomainException("msg");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

public sealed class UnauthorizedExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        var ex = new UnauthorizedException();
        Assert.False(string.IsNullOrWhiteSpace(ex.Message));
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new UnauthorizedException("not allowed");
        Assert.Equal("not allowed", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInner_SetsBoth()
    {
        var inner = new Exception("inner");
        var ex = new UnauthorizedException("not allowed", inner);
        Assert.Equal("not allowed", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void UnauthorizedException_IsException()
    {
        var ex = new UnauthorizedException();
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

public sealed class ProfileNotFoundExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        var ex = new ProfileNotFoundException();
        Assert.False(string.IsNullOrWhiteSpace(ex.Message));
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new ProfileNotFoundException("profile missing");
        Assert.Equal("profile missing", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInner_SetsBoth()
    {
        var inner = new Exception("inner");
        var ex = new ProfileNotFoundException("profile missing", inner);
        Assert.Equal("profile missing", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void ProfileNotFoundException_IsException()
    {
        var ex = new ProfileNotFoundException();
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
