#if !SqlDependency_Missing

using System.Threading;
using System.Threading.Tasks;
using Tortuga.Chain.Materializers;

#if SQL_SERVER_SDS

using System.Data.SqlClient;

#elif SQL_SERVER_MDS

using Microsoft.Data.SqlClient;

#endif

namespace Tortuga.Chain.SqlServer.CommandBuilders
{
	/// <summary>
	/// This is applied to command builders that support SqlDependency operations.
	/// </summary>
	internal interface ISupportsChangeListener
	{
		/// <summary>
		/// Waits for change in the data that is returned by this operation.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="state">User defined state, usually used for logging.</param>
		/// <returns>Task that can be waited for.</returns>
		/// <remarks>This requires the use of SQL Dependency</remarks>
		Task WaitForChange(CancellationToken cancellationToken, object? state = null);

		/// <summary>
		/// Prepares the command for execution by generating any necessary SQL.
		/// </summary>
		/// <param name="materializer">The materializer.</param>
		SqlServerCommandExecutionToken Prepare(Materializer<SqlCommand, SqlParameter> materializer);
	}
}

#endif
