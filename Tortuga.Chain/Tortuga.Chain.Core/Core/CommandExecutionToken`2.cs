using System.Data.Common;
using Tortuga.Chain.DataSources;

namespace Tortuga.Chain.Core
{
	/// <summary>
	/// This class represents the actual preparation and execution of a command.
	/// </summary>
	/// <typeparam name="TCommand">The type of the command used.</typeparam>
	/// <typeparam name="TParameter">The type of the t parameter type.</typeparam>
	/// <seealso cref="ExecutionToken" />
	public class CommandExecutionToken<TCommand, TParameter> : ExecutionToken
		where TCommand : DbCommand
		where TParameter : DbParameter
	{
		readonly ICommandDataSource<TCommand, TParameter> m_DataSource;

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandExecutionToken{TCommand, TParameter}"/> class.
		/// </summary>
		/// <param name="dataSource">The data source.</param>
		/// <param name="operationName">Name of the operation. This is used for logging.</param>
		/// <param name="commandText">The SQL to be executed.</param>
		/// <param name="parameters">The parameters.</param>
		/// <param name="commandType">Type of the command.</param>
		public CommandExecutionToken(ICommandDataSource<TCommand, TParameter> dataSource, string operationName, string commandText, IReadOnlyList<TParameter> parameters, CommandType commandType = CommandType.Text)
			: base(dataSource, operationName, commandText, commandType)
		{
			m_DataSource = dataSource;
			Parameters = parameters;
		}

		/// <summary>
		/// Gets the parameters.
		/// </summary>
		/// <value>The parameters.</value>
		public IReadOnlyList<TParameter> Parameters { get; }

		/// <summary>
		/// Applies the command overrides by calling OnBuildCommand, then firing the CommandBuilt event.
		/// </summary>
		/// <param name="command">The command.</param>
		public virtual void ApplyCommandOverrides(TCommand command)
		{
			OnBuildCommand(command);
			RaiseCommandBuild(command);
		}

		/// <summary>
		/// Executes the specified implementation.
		/// </summary>
		/// <param name="implementation">The implementation.</param>
		/// <param name="state">The state.</param>
		public int? Execute(CommandImplementation<TCommand> implementation, object? state)
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
		public Task<int?> ExecuteAsync(CommandImplementationAsync<TCommand> implementation, CancellationToken cancellationToken, object? state)
		{
			return m_DataSource.ExecuteAsync(this, implementation, cancellationToken, state);
		}

		/// <summary>
		/// Subclasses can override this method to change the command object after the command text and parameters are loaded.
		/// </summary>
		/// <param name="command">The command.</param>
		protected virtual void OnBuildCommand(TCommand command) { }
	}
}
