namespace HexMaster.Attendr.Core.Exceptions;

/// <summary>
/// Exception thrown when a user is not authenticated or the authentication token is missing required claims.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("User is not authenticated or authentication token is invalid.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
