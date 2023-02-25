using System.Data.Common;
using Tortuga.Chain.CommandBuilders;
using Tortuga.Chain.DataSources;
using Tortuga.Shipwright;

namespace Traits;

[Trait]
class SupportsSqlQueriesTrait<TCommand, TParameter> : ISupportsSqlQueries
	where TCommand : DbCommand
	where TParameter : DbParameter
{
	[Partial("sqlStatement,argumentValue")]
	public Func<string, object?, MultipleTableDbCommandBuilder<TCommand, TParameter>> OnSql { get; set; } = null!;

	IMultipleTableDbCommandBuilder ISupportsSqlQueries.Sql(string sqlStatement, object? argumentValue)
	{
		return OnSql(sqlStatement, argumentValue);
	}

	/// <summary>
	/// Creates a operation based on a raw SQL statement.
	/// </summary>
	/// <param name="sqlStatement">The SQL statement.</param>
	/// <returns></returns>
	[Expose]
	public MultipleTableDbCommandBuilder<TCommand, TParameter> Sql(string sqlStatement)
	{
		return OnSql(sqlStatement, null);
	}

	/// <summary>
	/// Creates a operation based on a raw SQL statement.
	/// </summary>
	/// <param name="sqlStatement">The SQL statement.</param>
	/// <param name="argumentValue">The argument value.</param>
	/// <returns>SqlServerSqlCall.</returns>
	[Expose]
	public MultipleTableDbCommandBuilder<TCommand, TParameter> Sql(string sqlStatement, object? argumentValue)
	{
		return OnSql(sqlStatement, argumentValue);
	}
}
