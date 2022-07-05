﻿using System.Collections.Immutable;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Tortuga.Chain.Core;
using Tortuga.Chain.Materializers;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.CommandBuilders;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
class GenericDbSqlCall : MultipleTableDbCommandBuilder<DbCommand, DbParameter>
{
	readonly object? m_ArgumentValue;
	readonly GenericDbDataSource m_DataSource;
	readonly string m_SqlStatement;

	/// <summary>
	/// Initializes a new instance of the <see cref="GenericDbSqlCall"/> class.
	/// </summary>
	/// <param name="dataSource">The data source.</param>
	/// <param name="sqlStatement">The SQL statement.</param>
	/// <param name="argumentValue">The argument value.</param>
	/// <exception cref="ArgumentException">sqlStatement is null or empty.;sqlStatement</exception>
	/// <exception cref="ArgumentException">sqlStatement is null or empty.;sqlStatement</exception>
	public GenericDbSqlCall(GenericDbDataSource dataSource, string sqlStatement, object? argumentValue) : base(dataSource)
	{
		if (string.IsNullOrEmpty(sqlStatement))
			throw new ArgumentException("sqlStatement is null or empty.", nameof(sqlStatement));

		m_SqlStatement = sqlStatement;
		m_ArgumentValue = argumentValue;
		m_DataSource = dataSource;
	}

	/// <summary>
	/// Prepares the command for execution by generating any necessary SQL.
	/// </summary>
	/// <param name="materializer">The materializer.</param>
	/// <returns>ExecutionToken&lt;TCommand&gt;.</returns>
	public override CommandExecutionToken<DbCommand, DbParameter> Prepare(Materializer<DbCommand, DbParameter> materializer)
	{
		var parameters = SqlBuilder.GetParameters(m_ArgumentValue, () => m_DataSource.CreateParameter());
		return new CommandExecutionToken<DbCommand, DbParameter>(DataSource, "Raw SQL call", m_SqlStatement, parameters);
	}

	/// <summary>
	/// Returns the column associated with the column name.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <returns></returns>
	/// <remarks>
	/// If the column name was not found, this will return null
	/// </remarks>
	public override ColumnMetadata? TryGetColumn(string columnName) => null;

	/// <summary>
	/// Returns a list of columns known to be non-nullable.
	/// </summary>
	/// <returns>
	/// If the command builder doesn't know which columns are non-nullable, an empty list will be returned.
	/// </returns>
	/// <remarks>
	/// This is used by materializers to skip IsNull checks.
	/// </remarks>
	public override IReadOnlyList<ColumnMetadata> TryGetNonNullableColumns()
	{
		return ImmutableList<ColumnMetadata>.Empty;
	}
}
