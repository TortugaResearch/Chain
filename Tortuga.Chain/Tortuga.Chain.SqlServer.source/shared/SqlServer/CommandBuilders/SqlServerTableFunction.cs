﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Tortuga.Chain.AuditRules;
using Tortuga.Chain.CommandBuilders;
using Tortuga.Chain.Core;
using Tortuga.Chain.Materializers;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.SqlServer.CommandBuilders
{
    /// <summary>
    /// Use for table-valued functions.
    /// </summary>
    /// <seealso cref="TableDbCommandBuilder{SqlCommand, SqlParameter, SqlServerLimitOption}" />
    internal class SqlServerTableFunction : TableDbCommandBuilder<SqlCommand, SqlParameter, SqlServerLimitOption>
    {
        readonly TableFunctionMetadata<SqlServerObjectName, SqlDbType> m_Table;
        readonly object m_FunctionArgumentValue;
        private object m_FilterValue;
        private string m_WhereClause;
        private object m_ArgumentValue;
        private IEnumerable<SortExpression> m_SortExpressions = Enumerable.Empty<SortExpression>();
        private SqlServerLimitOption m_LimitOptions;
        private int? m_Skip;
        private int? m_Take;
        private int? m_Seed;
        private string m_SelectClause;
        private FilterOptions m_FilterOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerTableFunction" /> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="tableFunctionName">Name of the table function.</param>
        /// <param name="functionArgumentValue">The function argument.</param>
        public SqlServerTableFunction(SqlServerDataSourceBase dataSource, SqlServerObjectName tableFunctionName, object functionArgumentValue) : base(dataSource)
        {
            m_Table = dataSource.DatabaseMetadata.GetTableFunction(tableFunctionName);
            m_FunctionArgumentValue = functionArgumentValue;
        }

        /// <summary>
        /// Adds sorting to the command builder.
        /// </summary>
        /// <param name="sortExpressions">The sort expressions.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public override TableDbCommandBuilder<SqlCommand, SqlParameter, SqlServerLimitOption> WithSorting(IEnumerable<SortExpression> sortExpressions)
        {
            if (sortExpressions == null)
                throw new ArgumentNullException(nameof(sortExpressions), $"{nameof(sortExpressions)} is null.");

            m_SortExpressions = sortExpressions;
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
        protected override TableDbCommandBuilder<SqlCommand, SqlParameter, SqlServerLimitOption> OnWithLimits(int? skip, int? take, SqlServerLimitOption limitOptions, int? seed)
        {
            m_Seed = seed;
            m_Skip = skip;
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
        protected override TableDbCommandBuilder<SqlCommand, SqlParameter, SqlServerLimitOption> OnWithLimits(int? skip, int? take, LimitOptions limitOptions, int? seed)
        {
            m_Seed = seed;
            m_Skip = skip;
            m_Take = take;
            m_LimitOptions = (SqlServerLimitOption)limitOptions;
            return this;
        }

        /// <summary>
        /// Adds (or replaces) the filter on this command builder.
        /// </summary>
        /// <param name="filterValue">The filter value.</param>
        /// <param name="filterOptions">The filter options.</param>
        /// <returns>TableDbCommandBuilder&lt;SqlCommand, SqlParameter, SqlServerLimitOption&gt;.</returns>
        public override TableDbCommandBuilder<SqlCommand, SqlParameter, SqlServerLimitOption> WithFilter(object filterValue, FilterOptions filterOptions = FilterOptions.None)
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
        /// <returns></returns>
        public override TableDbCommandBuilder<SqlCommand, SqlParameter, SqlServerLimitOption> WithFilter(string whereClause)
        {
            m_FilterValue = null;
            m_WhereClause = whereClause;
            m_ArgumentValue = null;
            return this;
        }

        /// <summary>
        /// Adds (or replaces) the filter on this command builder.
        /// </summary>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="argumentValue">The argument value.</param>
        /// <returns></returns>
        public override TableDbCommandBuilder<SqlCommand, SqlParameter, SqlServerLimitOption> WithFilter(string whereClause, object argumentValue)
        {
            m_FilterValue = null;
            m_WhereClause = whereClause;
            m_ArgumentValue = argumentValue;
            return this;
        }

        /// <summary>
        /// Prepares the command for execution by generating any necessary SQL.
        /// </summary>
        /// <param name="materializer">The materializer.</param>
        /// <returns>
        /// ExecutionToken&lt;TCommand&gt;.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override CommandExecutionToken<SqlCommand, SqlParameter> Prepare(Materializer<SqlCommand, SqlParameter> materializer)
        {
            if (materializer == null)
                throw new ArgumentNullException(nameof(materializer), $"{nameof(materializer)} is null.");

            var sqlBuilder = m_Table.CreateSqlBuilder(StrictMode);
            sqlBuilder.ApplyRulesForSelect(DataSource);

            if (m_FunctionArgumentValue != null)
                sqlBuilder.ApplyArgumentValue(DataSource, OperationTypes.None, m_FunctionArgumentValue);
            if (m_SelectClause == null)
                sqlBuilder.ApplyDesiredColumns(materializer.DesiredColumns());

            //Support check
            if (!Enum.IsDefined(typeof(SqlServerLimitOption), m_LimitOptions))
                throw new NotSupportedException($"SQL Server does not support limit option {(LimitOptions)m_LimitOptions}");
            if (m_LimitOptions == SqlServerLimitOption.TableSampleSystemRows || m_LimitOptions == SqlServerLimitOption.TableSampleSystemPercentage)
                throw new NotSupportedException($"SQL Server does not support limit option {(LimitOptions)m_LimitOptions} with table-valued functions");
            if (m_Seed.HasValue)
                throw new NotSupportedException($"SQL Server does not setting a random seed for table-valued functions");

            //Validation
            if (m_Skip < 0)
                throw new InvalidOperationException($"Cannot skip {m_Skip} rows");

            if (m_Skip > 0 && !m_SortExpressions.Any())
                throw new InvalidOperationException($"Cannot perform a Skip operation with out a sort expression.");

            if (m_Skip > 0 && m_LimitOptions != SqlServerLimitOption.Rows)
                throw new InvalidOperationException($"Cannot perform a Skip operation with limit option {m_LimitOptions}");

            if (m_Take <= 0)
                throw new InvalidOperationException($"Cannot take {m_Take} rows");

            if ((m_LimitOptions == SqlServerLimitOption.RowsWithTies || m_LimitOptions == SqlServerLimitOption.PercentageWithTies) && !m_SortExpressions.Any())
                throw new InvalidOperationException($"Cannot perform a WITH TIES operation without sorting.");

            //SQL Generation
            List<SqlParameter> parameters;
            var sql = new StringBuilder();

            string topClause = null;
            switch (m_LimitOptions)
            {
                case SqlServerLimitOption.Rows:
                    if (!m_SortExpressions.Any())
                        topClause = $"TOP (@fetch_row_count_expression) ";
                    break;
                case SqlServerLimitOption.Percentage:
                    topClause = $"TOP (@fetch_row_count_expression) PERCENT ";
                    break;
                case SqlServerLimitOption.PercentageWithTies:
                    topClause = $"TOP (@fetch_row_count_expression) PERCENT WITH TIES ";
                    break;
                case SqlServerLimitOption.RowsWithTies:
                    topClause = $"TOP (@fetch_row_count_expression) WITH TIES ";
                    break;
            }

            if (m_SelectClause != null)
                sql.Append($"SELECT {topClause} {m_SelectClause} ");
            else
                sqlBuilder.BuildSelectClause(sql, "SELECT " + topClause, null, null);

            sqlBuilder.BuildFromFunctionClause(sql, $" FROM {m_Table.Name.ToQuotedString()} (", " ) ");

            if (m_FilterValue != null)
            {
                sql.Append(" WHERE " + sqlBuilder.ApplyFilterValue(m_FilterValue, m_FilterOptions));
                sqlBuilder.BuildSoftDeleteClause(sql, " AND ", DataSource, null);

                parameters = sqlBuilder.GetParameters();
            }
            else if (!string.IsNullOrWhiteSpace(m_WhereClause))
            {
                sql.Append(" WHERE " + m_WhereClause);
                sqlBuilder.BuildSoftDeleteClause(sql, " AND ", DataSource, null);

                parameters = SqlBuilder.GetParameters<SqlParameter>(m_ArgumentValue);
                parameters.AddRange(sqlBuilder.GetParameters());
            }
            else
            {
                sqlBuilder.BuildSoftDeleteClause(sql, " WHERE ", DataSource, null);
                parameters = sqlBuilder.GetParameters();
            }
            sqlBuilder.BuildOrderByClause(sql, " ORDER BY ", m_SortExpressions, null);

            switch (m_LimitOptions)
            {
                case SqlServerLimitOption.Rows:
                    if (m_SortExpressions.Any())
                    {
                        sql.Append(" OFFSET @offset_row_count_expression ROWS ");
                        parameters.Add(new SqlParameter("@offset_row_count_expression", m_Skip ?? 0));

                        if (m_Take.HasValue)
                        {
                            sql.Append(" FETCH NEXT @fetch_row_count_expression ROWS ONLY");
                            parameters.Add(new SqlParameter("@fetch_row_count_expression", m_Take));
                        }
                    }
                    else
                    {
                        parameters.Add(new SqlParameter("@fetch_row_count_expression", m_Take));
                    }
                    break;

                case SqlServerLimitOption.Percentage:
                case SqlServerLimitOption.PercentageWithTies:
                case SqlServerLimitOption.RowsWithTies:
                    parameters.Add(new SqlParameter("@fetch_row_count_expression", m_Take));

                    break;
            }

            sql.Append(";");

            return new SqlServerCommandExecutionToken(DataSource, "Query Function " + m_Table.Name, sql.ToString(), parameters);
        }

        /// <summary>
        /// Returns the row count using a <c>SELECT Count(*)</c> style query.
        /// </summary>
        /// <returns></returns>
        public override ILink<long> AsCount()
        {
            m_SelectClause = "COUNT_BIG(*)";
            return ToInt64();
        }

        /// <summary>
        /// Returns the row count for a given column. <c>SELECT Count(columnName)</c>
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="distinct">if set to <c>true</c> use <c>SELECT COUNT(DISTINCT columnName)</c>.</param>
        /// <returns></returns>
        public override ILink<long> AsCount(string columnName, bool distinct = false)
        {
            var column = m_Table.Columns[columnName];
            if (distinct)
                m_SelectClause = $"COUNT_BIG(DISTINCT {column.QuotedSqlName})";
            else
                m_SelectClause = $"COUNT_BIG({column.QuotedSqlName})";

            return ToInt64();
        }

        /// <summary>
        /// Gets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public new SqlServerDataSourceBase DataSource
        {
            get { return (SqlServerDataSourceBase)base.DataSource; }
        }


        /// <summary>
        /// Returns the column associated with the column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the column name was not found, this will return null
        /// </remarks>
        public override ColumnMetadata TryGetColumn(string columnName)
        {
            return m_Table.Columns.TryGetColumn(columnName);
        }
    }
}
