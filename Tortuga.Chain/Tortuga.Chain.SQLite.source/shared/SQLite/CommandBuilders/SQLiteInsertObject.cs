﻿using System;
using System.Data.SQLite;
using System.Text;
using Tortuga.Chain.Core;
using Tortuga.Chain.Materializers;

namespace Tortuga.Chain.SQLite.CommandBuilders
{
    /// <summary>
    /// Class that represents a SQLite Insert.
    /// </summary>
    internal sealed class SQLiteInsertObject<TArgument> : SQLiteObjectCommand<TArgument>
        where TArgument : class
    {
        readonly InsertOptions m_Options;

        /// <summary>
        /// Initializes a new instance of <see cref="SQLiteInsertObject{TArgument}" /> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The options.</param>
        public SQLiteInsertObject(SQLiteDataSourceBase dataSource, SQLiteObjectName tableName, TArgument argumentValue, InsertOptions options)
            : base(dataSource, tableName, argumentValue)
        {
            m_Options = options;
        }

        /// <summary>
        /// Prepares the command for execution by generating any necessary SQL.
        /// </summary>
        /// <param name="materializer"></param>
        /// <returns><see cref="SQLiteCommandExecutionToken" /></returns>
        public override CommandExecutionToken<SQLiteCommand, SQLiteParameter> Prepare(Materializer<SQLiteCommand, SQLiteParameter> materializer)
        {
            if (materializer == null)
                throw new ArgumentNullException(nameof(materializer), $"{nameof(materializer)} is null.");

            var identityInsert = m_Options.HasFlag(InsertOptions.IdentityInsert);

            var sqlBuilder = Table.CreateSqlBuilder(StrictMode);
            sqlBuilder.ApplyArgumentValue(DataSource, ArgumentValue, m_Options);
            sqlBuilder.ApplyDesiredColumns(materializer.DesiredColumns());

            if (KeyColumns.Count > 0)
                sqlBuilder.OverrideKeys(KeyColumns);

            var sql = new StringBuilder();
            sqlBuilder.BuildInsertStatement(sql, Table.Name.ToQuotedString(), ";", identityInsert);
            sql.AppendLine();
            sqlBuilder.BuildSelectClause(sql, "SELECT ", null, $" FROM {Table.Name.ToQuotedString()} WHERE ROWID=last_insert_rowid();");

            return new SQLiteCommandExecutionToken(DataSource, "Insert into " + Table.Name, sql.ToString(), sqlBuilder.GetParameters(), lockType: LockType.Write);
        }
    }
}
