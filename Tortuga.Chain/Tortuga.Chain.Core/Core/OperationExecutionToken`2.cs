using System.Data.Common;
using Tortuga.Chain.DataSources;

namespace Tortuga.Chain.Core;

/// <summary>
/// This class represents the actual preparation and execution of a operation that isn't dependent on a DbCommand.
/// </summary>
/// <typeparam name="TConnection">The type of the t connection.</typeparam>
/// <typeparam name="TTransaction">The type of the t transaction.</typeparam>
/// <seealso cref="ExecutionToken" />
public class OperationExecutionToken<TConnection, TTransaction> : ExecutionToken
	where TConnection : DbConnection
	where TTransaction : DbTransaction
{
	readonly IOperationDataSource<TConnection, TTransaction> m_DataSource;

	/// <summary>
	/// Initializes a new instance of the <see cref="OperationExecutionToken{TConnection, TTransaction}"/> class.
	/// </summary>
	/// <param name="dataSource">The data source.</param>
	/// <param name="operationName">Name of the operation. This is used for logging.</param>
	public OperationExecutionToken(IOperationDataSource<TConnection, TTransaction> dataSource, string operationName)
		: base(dataSource, operationName, null, CommandType.Text)
	{
		m_DataSource = dataSource;
	}

	/// <summary>
	/// Executes the specified implementation.
	/// </summary>
	/// <param name="implementation">The implementation.</param>
	/// <param name="state">The state.</param>
	public int? Execute(OperationImplementation<TConnection, TTransaction> implementation, object? state)
	{
		return m_DataSource.Execute(this, implementation, state);
	}

	/// <summary>
	/// Executes the specified implementation asynchronously.
	/// </summary>
	/// <param name="implementation">The implementation.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <param name="state">The state.</param>
	/// <returns>Task.</returns>
	public Task<int?> ExecuteAsync(OperationImplementationAsync<TConnection, TTransaction> implementation, CancellationToken cancellationToken, object? state)
	{
		return m_DataSource.ExecuteAsync(this, implementation, cancellationToken, state);
	}
}
