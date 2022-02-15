using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Tortuga.Chain.CommandBuilders;

namespace Tortuga.Chain.Materializers
{
	/// <summary>
	/// Materializes the result set as a DataSet.
	/// </summary>
	/// <typeparam name="TCommand">The type of the t command type.</typeparam>
	/// <typeparam name="TParameter">The type of the t parameter type.</typeparam>
	internal sealed class DataSetMaterializer<TCommand, TParameter> : Materializer<TCommand, TParameter, DataSet> where TCommand : DbCommand
		where TParameter : DbParameter
	{
		readonly string[] m_TableNames;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataSetMaterializer{TCommand, TParameter}"/> class.
		/// </summary>
		/// <param name="commandBuilder">The command builder.</param>
		/// <param name="tableNames">The table names.</param>
		public DataSetMaterializer(DbCommandBuilder<TCommand, TParameter> commandBuilder, string[] tableNames)
			: base(commandBuilder)
		{
			m_TableNames = tableNames;
		}

		/// <summary>
		/// Returns the list of columns the materializer would like to have.
		/// </summary>
		/// <returns>
		/// IReadOnlyList&lt;System.String&gt;.
		/// </returns>
		/// <remarks>
		/// If AutoSelectDesiredColumns is returned, the command builder is allowed to choose which columns to return. If NoColumns is returned, the command builder should omit the SELECT/OUTPUT clause.
		/// </remarks>
		public override IReadOnlyList<string> DesiredColumns() => AllColumns;

		/// <summary>
		/// Execute the operation synchronously.
		/// </summary>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		public override DataSet Execute(object? state = null)
		{
			var ds = new DataSet() { EnforceConstraints = false /*needed for PostgreSql*/};
			Prepare().Execute(cmd =>
			 {
				 using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
				 {
					 ds.Load(reader, LoadOption.OverwriteChanges, m_TableNames);
					 return ds.Tables.Cast<DataTable>().Sum(t => t.Rows.Count);
				 }
			 }, state);

			return ds;
		}

		/// <summary>
		/// Execute the operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="state">User defined state, usually used for logging.</param>
		/// <returns></returns>
		public override async Task<DataSet> ExecuteAsync(CancellationToken cancellationToken, object? state = null)
		{
			var ds = new DataSet() { EnforceConstraints = false /*needed for PostgreSql*/};
			await Prepare().ExecuteAsync(async cmd =>
			{
				using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken).ConfigureAwait(false))
				{
					ds.Load(reader, LoadOption.OverwriteChanges, m_TableNames);
					return ds.Tables.Cast<DataTable>().Sum(t => t.Rows.Count);
				}
			}, cancellationToken, state).ConfigureAwait(false);

			return ds;
		}
	}
}
