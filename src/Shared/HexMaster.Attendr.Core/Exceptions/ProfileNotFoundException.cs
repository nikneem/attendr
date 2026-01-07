namespace HexMaster.Attendr.Core.Exceptions;

/// <summary>
/// Exception thrown when a user profile cannot be found or resolved from the authenticated user.
/// </summary>
public class ProfileNotFoundException : Exception
{
    public ProfileNotFoundException()
        : base("User profile not found. Please create a profile first.")
    {
    }

    public ProfileNotFoundException(string message)
        : base(message)
    {
    }

    public ProfileNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
