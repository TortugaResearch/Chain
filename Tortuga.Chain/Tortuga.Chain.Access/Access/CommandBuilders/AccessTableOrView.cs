﻿using System.Data.OleDb;
using System.Text;
using Tortuga.Chain.CommandBuilders;
using Tortuga.Chain.Core;
using Tortuga.Chain.Materializers;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.Access.CommandBuilders;

/// <summary>
/// AccessTableOrView supports queries against tables and views.
/// </summary>
internal sealed class AccessTableOrView<TObject> : TableDbCommandBuilder<OleDbCommand, OleDbParameter, AccessLimitOption, TObject>
	where TObject : class
{
	readonly TableOrViewMetadata<AccessObjectName, OleDbType> m_Table;
	object? m_ArgumentValue;
	FilterOptions m_FilterOptions;
	object? m_FilterValue;
	AccessLimitOption m_LimitOptions;
	string? m_SelectClause;
	IEnumerable<SortExpression> m_SortExpressions = Enumerable.Empty<SortExpression>();
	int? m_Take;
	string? m_WhereClause;

	/// <summary>
	/// Initializes a new instance of the <see cref="AccessTableOrView{TObject}" /> class.
	/// </summary>
	/// <param name="dataSource">The data source.</param>
	/// <param name="tableOrViewName">Name of the table or view.</param>
	/// <param name="filterValue">The filter value.</param>
	/// <param name="filterOptions">The filter options.</param>
	public AccessTableOrView(AccessDataSourceBase dataSource, AccessObjectName tableOrViewName, object filterValue, FilterOptions filterOptions) :
		base(dataSource)
	{
		m_FilterValue = filterValue;
		m_Table = ((AccessDataSourceBase)DataSource).DatabaseMetadata.GetTableOrView(tableOrViewName);
		m_FilterOptions = filterOptions;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AccessTableOrView{TObject}" /> class.
	/// </summary>
	/// <param name="dataSource"></param>
	/// <param name="tableOrViewName"></param>
	/// <param name="whereClause"></param>
	/// <param name="argumentValue"></param>
	public AccessTableOrView(AccessDataSourceBase dataSource, AccessObjectName tableOrViewName, string? whereClause, object? argumentValue) :
		base(dataSource)
	{
		m_ArgumentValue = argumentValue;
		m_WhereClause = whereClause;
		m_Table = ((AccessDataSourceBase)DataSource).DatabaseMetadata.GetTableOrView(tableOrViewName);
	}

	/// <summary>
	/// Gets the default limit option.
	/// </summary>
	/// <value>
	/// The default limit options.
	/// </value>
	/// <remarks>
	/// For most data sources, this will be LimitOptions.Rows.
	/// </remarks>
	protected override LimitOptions DefaultLimitOption => LimitOptions.RowsWithTies;

	/// <summary>
	/// Returns the row count using a <c>SELECT COUNT_BIG(*)</c> style query.
	/// </summary>
	/// <returns></returns>
	public override ILink<long> AsCount()
	{
		m_SelectClause = "COUNT(*)";
		return ToInt64();
	}

	/// <summary>
	/// Returns the row count for a given column. <c>SELECT COUNT_BIG(columnName)</c>
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <param name="distinct">if set to <c>true</c> use <c>SELECT COUNT_BIG(DISTINCT columnName)</c>.</param>
	/// <returns></returns>
	public override ILink<long> AsCount(string columnName, bool distinct = false)
	{
		if (distinct)
			throw new NotSupportedException("Access does not support distinct counts.");

		var column = m_Table.Columns[columnName];
		m_SelectClause = $"COUNT({column.QuotedSqlName})";

		return ToInt64();
	}

	/// <summary>
	/// Prepares the command for execution by generating any necessary SQL.
	/// </summary>
	/// <param name="materializer"></param>
	/// <returns></returns>
	public override CommandExecutionToken<OleDbCommand, OleDbParameter> Prepare(Materializer<OleDbCommand, OleDbParameter> materializer)
	{
		if (materializer == null)
			throw new ArgumentNullException(nameof(materializer), $"{nameof(materializer)} is null.");

		var sqlBuilder = m_Table.CreateSqlBuilder(StrictMode);
		sqlBuilder.ApplyRulesForSelect(DataSource);
		sqlBuilder.ApplyDesiredColumns(materializer.DesiredColumns());

		//Support check
		if (!Enum.IsDefined(typeof(AccessLimitOption), m_LimitOptions))
			throw new NotSupportedException($"Access does not support limit option {(LimitOptions)m_LimitOptions}");

		//Validation

		if (m_Take <= 0)
			throw new InvalidOperationException($"Cannot take {m_Take} rows");

		//SQL Generation
		List<OleDbParameter> parameters;
		var sql = new StringBuilder();

		string? topClause = null;
		switch (m_LimitOptions)
		{
			case AccessLimitOption.RowsWithTies:
				topClause = $"TOP {m_Take} ";
				break;
		}

		if (m_SelectClause != null)
			sql.Append($"SELECT {topClause} {m_SelectClause} ");
		else
			sqlBuilder.BuildSelectClause(sql, "SELECT " + topClause, null, null);

		sql.Append(" FROM " + m_Table.Name.ToQuotedString());

		if (m_FilterValue != null)
		{
			sql.Append(" WHERE (" + sqlBuilder.ApplyFilterValue(m_FilterValue, m_FilterOptions) + ")");
			sqlBuilder.BuildSoftDeleteClause(sql, " AND (", DataSource, ") ");

			parameters = sqlBuilder.GetParameters();
		}
		else if (!string.IsNullOrWhiteSpace(m_WhereClause))
		{
			sql.Append(" WHERE (" + m_WhereClause + ")");
			sqlBuilder.BuildSoftDeleteClause(sql, " AND (", DataSource, ") ");

			parameters = SqlBuilder.GetParameters<OleDbParameter>(m_ArgumentValue);
			parameters.AddRange(sqlBuilder.GetParameters());
		}
		else
		{
			sqlBuilder.BuildSoftDeleteClause(sql, " WHERE ", DataSource, null);
			parameters = sqlBuilder.GetParameters();
		}
		sqlBuilder.BuildOrderByClause(sql, " ORDER BY ", m_SortExpressions, null);

		sql.Append(";");

		return new AccessCommandExecutionToken(DataSource, "Query " + m_Table.Name, sql.ToString(), parameters);
	}

	/// <summary>
	/// Returns the column associated with the column name.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <returns></returns>
	/// <remarks>
	/// If the column name was not found, this will return null
	/// </remarks>
	public override ColumnMetadata? TryGetColumn(string columnName) => m_Table.Columns.TryGetColumn(columnName);

	/// <summary>
	/// Returns a list of columns known to be non-nullable.
	/// </summary>
	/// <returns>
	/// If the command builder doesn't know which columns are non-nullable, an empty list will be returned.
	/// </returns>
	/// <remarks>
	/// This is used by materializers to skip IsNull checks.
	/// </remarks>
	public override IReadOnlyList<ColumnMetadata> TryGetNonNullableColumns() => m_Table.NonNullableColumns;

	/// <summary>
	/// Adds (or replaces) the filter on this command builder.
	/// </summary>
	/// <param name="filterValue">The filter value.</param>
	/// <param name="filterOptions">The filter options.</param>
	/// <returns>TableDbCommandBuilder&lt;OleDbCommand, OleDbParameter, AccessLimitOption&gt;.</returns>
	protected override TableDbCommandBuilder<OleDbCommand, OleDbParameter, AccessLimitOption> OnWithFilter(object filterValue, FilterOptions filterOptions = FilterOptions.None)
	{
		m_FilterValue = filterValue;
		m_WhereClause = null;
		m_ArgumentValue = null;
		m_FilterOptions = filterOptions;
		return this;
	}

	/// <summary>
	/// Adds (or replaces) the filter on this command builder.
	/// </summary>
	/// <param name="whereClause">The where clause.</param>
	/// <param name="argumentValue">The argument value.</param>
	/// <returns></returns>
	protected override TableDbCommandBuilder<OleDbCommand, OleDbParameter, AccessLimitOption> OnWithFilter(string whereClause, object? argumentValue)
	{
		m_FilterValue = null;
		m_WhereClause = whereClause;
		m_ArgumentValue = argumentValue;
		return this;
	}

	/// <summary>
	/// Adds limits to the command builder.
	/// </summary>
	/// <param name="skip">The number of rows to skip.</param>
	/// <param name="take">Number of rows to take.</param>
	/// <param name="limitOptions">The limit options.</param>
	/// <param name="seed">The seed for repeatable reads. Only applies to random sampling</param>
	/// <returns></returns>
	protected override TableDbCommandBuilder<OleDbCommand, OleDbParameter, AccessLimitOption> OnWithLimits(int? skip, int? take, AccessLimitOption limitOptions, int? seed)
	{
		if (skip.HasValue)
			throw new NotSupportedException("Row skipping isn't supported by Access");
		if (seed.HasValue)
			throw new NotSupportedException("Seed values are not supported by Access");

		m_Take = take;
		m_LimitOptions = limitOptions;
		return this;
	}

	/// <summary>
	/// Adds limits to the command builder.
	/// </summary>
	/// <param name="skip">The number of rows to skip.</param>
	/// <param name="take">Number of rows to take.</param>
	/// <param name="limitOptions">The limit options.</param>
	/// <param name="seed">The seed for repeatable reads. Only applies to random sampling</param>
	/// <returns></returns>
	protected override TableDbCommandBuilder<OleDbCommand, OleDbParameter, AccessLimitOption> OnWithLimits(int? skip, int? take, LimitOptions limitOptions, int? seed)
	{
		if (skip.HasValue)
			throw new NotSupportedException("Row skipping isn't supported by Access");
		if (seed.HasValue)
			throw new NotSupportedException("Seed values are not supported by Access");

		m_Take = take;
		m_LimitOptions = (AccessLimitOption)limitOptions;
		return this;
	}

	/// <summary>
	/// Adds sorting to the command builder.
	/// </summary>
	/// <param name="sortExpressions">The sort expressions.</param>
	/// <returns></returns>
	protected override TableDbCommandBuilder<OleDbCommand, OleDbParameter, AccessLimitOption> OnWithSorting(IEnumerable<SortExpression> sortExpressions)
	{
		m_SortExpressions = sortExpressions ?? throw new ArgumentNullException(nameof(sortExpressions), $"{nameof(sortExpressions)} is null.");
		return this;
	}
}
