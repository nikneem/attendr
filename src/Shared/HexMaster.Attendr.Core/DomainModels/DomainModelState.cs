namespace HexMaster.Attendr.Core.DomainModels;

/// <summary>
/// Represents the state of a domain model in its lifecycle.
/// </summary>
public enum DomainModelState
{
    /// <summary>
    /// The domain model was just created (e.g., via a POST request).
    /// This is a terminal state - objects in Created state cannot change state.
    /// </summary>
    Created,

    /// <summary>
    /// The domain model was loaded from the database and has not been modified.
    /// Can transition to: Touched, Modified, Deleted.
    /// </summary>
    Pristine,

    /// <summary>
    /// A property setter was called, but the value did not actually change.
    /// Can transition to: Modified, Deleted.
    /// </summary>
    Touched,

    /// <summary>
    /// The domain model has been modified (actual values changed).
    /// Can transition to: Deleted.
    /// </summary>
    Modified,

    /// <summary>
    /// The domain model has been marked for deletion.
    /// This is a terminal state - objects in Deleted state cannot change state.
    /// </summary>
    Deleted
}
