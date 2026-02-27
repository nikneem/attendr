namespace HexMaster.Attendr.Core.Exceptions;

/// <summary>
/// Exception thrown when an authenticated user attempts an operation they are not permitted to perform.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    /// <summary>
    /// Gets a stable error type URI identifying this specific kind of forbidden situation.
    /// </summary>
    public string ErrorType { get; }

    public ForbiddenException()
        : base("Access to this resource or operation is forbidden.")
    {
        ErrorType = "https://attendr.dev/errors/forbidden";
    }

    public ForbiddenException(string message, string? errorType = null)
        : base(message)
    {
        ErrorType = errorType ?? "https://attendr.dev/errors/forbidden";
    }

    public ForbiddenException(string message, Exception innerException, string? errorType = null)
        : base(message, innerException)
    {
        ErrorType = errorType ?? "https://attendr.dev/errors/forbidden";
    }
}
