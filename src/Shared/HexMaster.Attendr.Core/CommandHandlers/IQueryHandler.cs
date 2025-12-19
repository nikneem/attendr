namespace HexMaster.Attendr.Core.CommandHandlers;

/// <summary>
/// Handler interface for queries that return a result.
/// Shared CQRS abstraction across all modules for read operations.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface IQueryHandler<TQuery, TResult>
{
    /// <summary>
    /// Handles the query and returns a result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns the result.</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
}
