namespace HexMaster.Attendr.Core.CommandHandlers;

/// <summary>
/// Handler interface for commands that return a result.
/// Shared CQRS abstraction across all modules.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface ICommandHandler<TCommand, TResult>
{
    /// <summary>
    /// Handles the command and returns a result.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns the result.</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler interface for commands that do not return a result.
/// Shared CQRS abstraction across all modules.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<TCommand>
{
    /// <summary>
    /// Handles the command without returning a result.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}
