namespace HexMaster.Attendr.Core.Constants;

/// <summary>
/// Contains constants for pagination across the Attendr application.
/// </summary>
public static class PaginationConstants
{
    /// <summary>
    /// The default page size when no page size is specified.
    /// </summary>
    public const int DefaultPageSize = 25;

    /// <summary>
    /// The maximum allowed page size.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// The minimum allowed page size.
    /// </summary>
    public const int MinPageSize = 1;

    /// <summary>
    /// Validates and normalizes a page size value.
    /// </summary>
    /// <param name="pageSize">The requested page size (null for default).</param>
    /// <returns>A validated page size between MinPageSize and MaxPageSize.</returns>
    public static int NormalizePageSize(int? pageSize)
    {
        if (!pageSize.HasValue)
        {
            return DefaultPageSize;
        }

        return Math.Max(MinPageSize, Math.Min(pageSize.Value, MaxPageSize));
    }
}
