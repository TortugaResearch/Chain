using System.Data.Common;
using Tortuga.Chain.Core;

namespace Tortuga.Chain.DataSources;

/// <summary>
/// Class DataSource.
/// </summary>
/// <typeparam name="TConnection">The type of the t connection.</typeparam>
/// <typeparam name="TTransaction">The type of the t transaction.</typeparam>
/// <typeparam name="TCommand">The type of the command used.</typeparam>
/// <typeparam name="TParameter">The type of the t parameter type.</typeparam>
/// <seealso cref="DataSource" />
public abstract class DataSource<TConnection, TTransaction, TCommand, TParameter> : DataSource, ICommandDataSource<TCommand, TParameter>, IOperationDataSource<TConnection, TTransaction>
    where TConnection : DbConnection
    where TTransaction : DbTransaction
    where TCommand : DbCommand
    where TParameter : DbParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSource{TConnection, TTransaction,TCommand, TParameter}"/> class.
    /// </summary>
    /// <param name="settings">Optional settings object.</param>
    protected DataSource(DataSourceSettings? settings) : base(settings) { }

    int? IOperationDataSource<TConnection, TTransaction>.Execute(OperationExecutionToken<TConnection, TTransaction> executionToken, OperationImplementation<TConnection, TTransaction> implementation, object? state)
    {
        return Execute(executionToken, implementation, state);
    }

    int? ICommandDataSource<TCommand, TParameter>.Execute(CommandExecutionToken<TCommand, TParameter> executionToken, CommandImplementation<TCommand> implementation, object? state)
    {
        return Execute(executionToken, implementation, state);
    }

    Task<int?> ICommandDataSource<TCommand, TParameter>.ExecuteAsync(CommandExecutionToken<TCommand, TParameter> executionToken, CommandImplementationAsync<TCommand> implementation, CancellationToken cancellationToken, object? state)
    {
        return ExecuteAsync(executionToken, implementation, cancellationToken, state);
    }

    Task<int?> IOperationDataSource<TConnection, TTransaction>.ExecuteAsync(OperationExecutionToken<TConnection, TTransaction> executionToken, OperationImplementationAsync<TConnection, TTransaction> implementation, CancellationToken cancellationToken, object? state)
    {
        return ExecuteAsync(executionToken, implementation, cancellationToken, state);
    }

    /// <summary>
    /// Executes the specified implementation.
    /// </summary>
    /// <param name="executionToken">The execution token.</param>
    /// <param name="implementation">The implementation.</param>
    /// <param name="state">The state.</param>
    /// <returns>The caller is expected to use the StreamingCommandCompletionToken to close any lingering connections and fire appropriate events.</returns>
    public abstract StreamingCommandCompletionToken ExecuteStream(CommandExecutionToken<TCommand, TParameter> executionToken, StreamingCommandImplementation<TCommand> implementation, object? state);

    /// <summary>
    /// Executes the specified implementation asynchronously.
    /// </summary>
    /// <param name="executionToken">The execution token.</param>
    /// <param name="implementation">The implementation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="state">The state.</param>
    /// <returns>The caller is expected to use the StreamingCommandCompletionToken to close any lingering connections and fire appropriate events.</returns>
    public abstract Task<StreamingCommandCompletionToken> ExecuteStreamAsync(CommandExecutionToken<TCommand, TParameter> executionToken, StreamingCommandImplementationAsync<TCommand> implementation, CancellationToken cancellationToken, object? state);

    /// <summary>
    /// Executes the specified operation.
    /// </summary>
    /// <param name="executionToken">The execution token.</param>
    /// <param name="implementation">The implementation that handles processing the result of the command.</param>
    /// <param name="state">User supplied state.</param>
    protected internal abstract int? Execute(CommandExecutionToken<TCommand, TParameter> executionToken, CommandImplementation<TCommand> implementation, object? state);

    /// <summary>
    /// Executes the specified operation.
    /// </summary>
    /// <param name="executionToken">The execution token.</param>
    /// <param name="implementation">The implementation.</param>
    /// <param name="state">The state.</param>
    protected internal abstract int? Execute(OperationExecutionToken<TConnection, TTransaction> executionToken, OperationImplementation<TConnection, TTransaction> implementation, object? state);

    /// <summary>
    /// Executes the operation asynchronously.
    /// </summary>
    /// <param name="executionToken">The execution token.</param>
    /// <param name="implementation">The implementation that handles processing the result of the command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="state">User supplied state.</param>
    /// <returns>Task.</returns>
    protected internal abstract Task<int?> ExecuteAsync(CommandExecutionToken<TCommand, TParameter> executionToken, CommandImplementationAsync<TCommand> implementation, CancellationToken cancellationToken, object? state);

    /// <summary>
    /// Execute the operation asynchronously.
    /// </summary>
    /// <param name="executionToken">The execution token.</param>
    /// <param name="implementation">The implementation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="state">The state.</param>
    /// <returns>Task.</returns>
    protected internal abstract Task<int?> ExecuteAsync(OperationExecutionToken<TConnection, TTransaction> executionToken, OperationImplementationAsync<TConnection, TTransaction> implementation, CancellationToken cancellationToken, object? state);
}
