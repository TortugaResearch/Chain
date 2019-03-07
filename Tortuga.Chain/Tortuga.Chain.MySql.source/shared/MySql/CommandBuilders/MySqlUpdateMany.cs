﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Chain.CommandBuilders;
using Tortuga.Chain.Core;
using Tortuga.Chain.Materializers;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.MySql.CommandBuilders
{
    /// <summary>
    /// Class MySqlUpdateMany.
    /// </summary>
    internal sealed class MySqlUpdateMany : UpdateManyDbCommandBuilder<MySqlCommand, MySqlParameter>
    {
        readonly int? m_ExpectedRowCount;
        readonly object m_NewValues;
        readonly UpdateOptions m_Options;
        readonly IEnumerable<MySqlParameter> m_Parameters;
        readonly TableOrViewMetadata<MySqlObjectName, MySqlDbType> m_Table;
        readonly object m_UpdateArgumentValue;
        readonly string m_UpdateExpression;
        FilterOptions m_FilterOptions;
        object m_FilterValue;
        object m_WhereArgumentValue;
        string m_WhereClause;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlUpdateMany" /> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="newValues">The new values.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="expectedRowCount">The expected row count.</param>
        /// <param name="options">The options.</param>
        public MySqlUpdateMany(MySqlDataSourceBase dataSource, MySqlObjectName tableName, object newValues, string whereClause, IEnumerable<MySqlParameter> parameters, int? expectedRowCount, UpdateOptions options) : base(dataSource)
        {
            if (options.HasFlag(UpdateOptions.UseKeyAttribute))
                throw new NotSupportedException("Cannot use Key attributes with this operation.");

            m_Table = dataSource.DatabaseMetadata.GetTableOrView(tableName);
            m_NewValues = newValues;
            m_WhereClause = whereClause;
            m_ExpectedRowCount = expectedRowCount;
            m_Options = options;
            m_Parameters = parameters;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlUpdateMany" /> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="newValues">The new values.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.NotSupportedException">Cannot use Key attributes with this operation.</exception>
        public MySqlUpdateMany(MySqlDataSourceBase dataSource, MySqlObjectName tableName, object newValues, UpdateOptions options) : base(dataSource)
        {
            if (options.HasFlag(UpdateOptions.UseKeyAttribute))
                throw new NotSupportedException("Cannot use Key attributes with this operation.");

            m_Table = dataSource.DatabaseMetadata.GetTableOrView(tableName);
            m_NewValues = newValues;
            m_Options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlUpdateMany" /> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="updateExpression">The update expression.</param>
        /// <param name="updateArgumentValue">The update argument value.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.NotSupportedException">Cannot use Key attributes with this operation.</exception>
        public MySqlUpdateMany(MySqlDataSourceBase dataSource, MySqlObjectName tableName, string updateExpression, object updateArgumentValue, UpdateOptions options) : base(dataSource)
        {
            if (options.HasFlag(UpdateOptions.UseKeyAttribute))
                throw new NotSupportedException("Cannot use Key attributes with this operation.");

            m_Table = dataSource.DatabaseMetadata.GetTableOrView(tableName);
            m_UpdateExpression = updateExpression;
            m_Options = options;
            m_UpdateArgumentValue = updateArgumentValue;
        }

        /// <summary>
        /// Applies this command to all rows.
        /// </summary>
        /// <returns></returns>
        public override UpdateManyDbCommandBuilder<MySqlCommand, MySqlParameter> All()
        {
            m_WhereClause = null;
            m_WhereArgumentValue = null;
            m_FilterValue = null;
            m_FilterOptions = FilterOptions.None;
            return this;
        }

        public override CommandExecutionToken<MySqlCommand, MySqlParameter> Prepare(Materializer<MySqlCommand, MySqlParameter> materializer)
        {
            if (materializer == null)
                throw new ArgumentNullException(nameof(materializer), $"{nameof(materializer)} is null.");

            SqlBuilder.CheckForOverlaps(m_NewValues, m_WhereArgumentValue, "The same parameter '{0}' appears in both the newValue object and the where clause argument. Rename the parameter in the where expression to resolve the conflict.");
            SqlBuilder.CheckForOverlaps(m_NewValues, m_FilterValue, "The same parameter '{0}' appears in both the newValue object and the filter object. Use an update expression or where expression to resolve the conflict.");
            SqlBuilder.CheckForOverlaps(m_UpdateArgumentValue, m_WhereArgumentValue, "The same parameter '{0}' appears in both the update expression argument and the where clause argument. Rename the parameter in the where expression to resolve the conflict.");
            SqlBuilder.CheckForOverlaps(m_UpdateArgumentValue, m_FilterValue, "The same parameter '{0}' appears in both the update expression argument and the filter object. Use an update expression or where expression to resolve the conflict.");

            var sqlBuilder = m_Table.CreateSqlBuilder(StrictMode);
            sqlBuilder.ApplyArgumentValue(DataSource, m_NewValues, m_Options);
            sqlBuilder.ApplyDesiredColumns(materializer.DesiredColumns());

            var sql = new StringBuilder();
            if (sqlBuilder.HasReadFields && m_Options.HasFlag(UpdateOptions.ReturnOldValues))
            {
                sqlBuilder.BuildSelectClause(sql, "SELECT ", null, " FROM " + m_Table.Name.ToQuotedString());
                if (m_FilterValue != null)
                    sql.Append(" WHERE " + sqlBuilder.ApplyFilterValue(m_FilterValue, m_FilterOptions));
                else if (!string.IsNullOrWhiteSpace(m_WhereClause))
                    sql.Append(" WHERE " + m_WhereClause);
                sql.AppendLine(";");
            }

            var parameters = new List<MySqlParameter>();
            sql.Append("UPDATE " + m_Table.Name.ToQuotedString());
            if (m_UpdateExpression == null)
            {
                sqlBuilder.BuildSetClause(sql, " SET ", null, null);
            }
            else
            {
                sql.Append(" SET " + m_UpdateExpression);
                parameters.AddRange(SqlBuilder.GetParameters<MySqlParameter>(m_UpdateArgumentValue));
            }

            if (m_FilterValue != null)
            {
                sql.Append(" WHERE " + sqlBuilder.ApplyFilterValue(m_FilterValue, m_FilterOptions));
                parameters.AddRange(sqlBuilder.GetParameters());
            }
            else if (!string.IsNullOrWhiteSpace(m_WhereClause))
            {
                sql.Append(" WHERE " + m_WhereClause);
                parameters.AddRange(SqlBuilder.GetParameters<MySqlParameter>(m_WhereArgumentValue));
                parameters.AddRange(sqlBuilder.GetParameters());
            }
            else
            {
                parameters = sqlBuilder.GetParameters();
            }
            sql.AppendLine(";");


            if (sqlBuilder.HasReadFields && !m_Options.HasFlag(UpdateOptions.ReturnOldValues))
            {
                sqlBuilder.BuildSelectClause(sql, "SELECT ", null, " FROM " + m_Table.Name.ToQuotedString());
                if (m_FilterValue != null)
                    sql.Append(" WHERE " + sqlBuilder.ApplyFilterValue(m_FilterValue, m_FilterOptions));
                else if (!string.IsNullOrWhiteSpace(m_WhereClause))
                    sql.Append(" WHERE " + m_WhereClause);
                sql.AppendLine(";");
            }

            if (m_Parameters != null)
                parameters.AddRange(m_Parameters);

            return new MySqlCommandExecutionToken(DataSource, "Update " + m_Table.Name, sql.ToString(), parameters).CheckUpdateRowCount(m_Options, m_ExpectedRowCount);
        }



        /// <summary>
        /// Returns the column associated with the column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        /// <remarks>
        /// If the column name was not found, this will return null
        /// </remarks>
        public override ColumnMetadata TryGetColumn(string columnName) => m_Table.Columns.TryGetColumn(columnName);

        /// <summary>
        /// Adds (or replaces) the filter on this command builder.
        /// </summary>
        /// <param name="filterValue">The filter value.</param>
        /// <param name="filterOptions">The filter options.</param>
        public override UpdateManyDbCommandBuilder<MySqlCommand, MySqlParameter> WithFilter(object filterValue, FilterOptions filterOptions = FilterOptions.None)
        {
            m_WhereClause = null;
            m_WhereArgumentValue = null;
            m_FilterValue = filterValue;
            m_FilterOptions = filterOptions;
            return this;
        }

        /// <summary>
        /// Adds (or replaces) the filter on this command builder.
        /// </summary>
        /// <param name="whereClause">The where clause.</param>
        /// <returns></returns>
        public override UpdateManyDbCommandBuilder<MySqlCommand, MySqlParameter> WithFilter(string whereClause)
        {
            m_WhereClause = whereClause;
            m_WhereArgumentValue = null;
            m_FilterValue = null;
            m_FilterOptions = FilterOptions.None;
            return this;
        }

        /// <summary>
        /// Adds (or replaces) the filter on this command builder.
        /// </summary>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="argumentValue">The argument value.</param>
        /// <returns></returns>
        public override UpdateManyDbCommandBuilder<MySqlCommand, MySqlParameter> WithFilter(string whereClause, object argumentValue)
        {
            m_WhereClause = whereClause;
            m_WhereArgumentValue = argumentValue;
            m_FilterValue = null;
            m_FilterOptions = FilterOptions.None;
            return this;
        }

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
    }
}


