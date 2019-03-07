﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tortuga.Anchor;
using Tortuga.Anchor.Metadata;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.SQLite
{
    /// <summary>
    /// Handles caching of metadata for various SQLite tables and views.
    /// </summary>
    public sealed class SQLiteMetadataCache : DatabaseMetadataCache<SQLiteObjectName, DbType>
    {
        readonly SQLiteConnectionStringBuilder m_ConnectionBuilder;
        readonly ConcurrentDictionary<SQLiteObjectName, TableOrViewMetadata<SQLiteObjectName, DbType>> m_Tables = new ConcurrentDictionary<SQLiteObjectName, TableOrViewMetadata<SQLiteObjectName, DbType>>();

        readonly ConcurrentDictionary<Type, TableOrViewMetadata<SQLiteObjectName, DbType>> m_TypeTableMap = new ConcurrentDictionary<Type, TableOrViewMetadata<SQLiteObjectName, DbType>>();

        /// <summary>
        /// Creates a new instance of <see cref="SQLiteMetadataCache"/>
        /// </summary>
        /// <param name="connectionBuilder">The connection builder.</param>
        public SQLiteMetadataCache(SQLiteConnectionStringBuilder connectionBuilder)
        {
            m_ConnectionBuilder = connectionBuilder;
        }

        public override IndexMetadataCollection<SQLiteObjectName> GetIndexesForTable(SQLiteObjectName tableName)
        {
            var table = GetTableOrView(tableName);

            var indexSql = $"PRAGMA index_list('{tableName.Name}')";
            var results = new List<IndexMetadata<SQLiteObjectName>>();
            using (var con = new SQLiteConnection(m_ConnectionBuilder.ConnectionString))
            using (var con2 = new SQLiteConnection(m_ConnectionBuilder.ConnectionString))
            {
                con.Open();
                con2.Open();
                using (var cmd = new SQLiteCommand(indexSql, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(reader.GetOrdinal("name"));
                        var isUnique = reader.GetInt64(reader.GetOrdinal("unique")) != 0;
                        var origin = reader.GetString(reader.GetOrdinal("origin"));
                        var isPrimaryKey = origin == "pk";
                        var isUniqueConstraint = origin == "u";

                        var columns = new List<IndexColumnMetadata>();

                        using (var cmd2 = new SQLiteCommand($"PRAGMA index_xinfo('{name}')", con2))
                        using (var reader2 = cmd2.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                var colName = (reader2.IsDBNull(reader2.GetOrdinal("name"))) ? null : reader2.GetString(reader2.GetOrdinal("name"));
                                var isDescending = reader2.GetInt64(reader2.GetOrdinal("desc")) != 0;
                                var isIncluded = reader2.GetInt64(reader2.GetOrdinal("key")) == 0;

                                ColumnMetadata<DbType> column;
                                if (colName != null)
                                    column = table.Columns.SingleOrDefault(c => c.SqlName == colName);
                                else //a null column name is really the ROWID
                                {
                                    column = table.Columns.SingleOrDefault(c => c.SqlName == "ROWID");

                                    //The ROWID may be aliased as the primary key
                                    column = table.PrimaryKeyColumns.Single();
                                }

                                columns.Add(new IndexColumnMetadata(column, isDescending, isIncluded));
                            }
                        }

                        results.Add(new IndexMetadata<SQLiteObjectName>(tableName, name, isPrimaryKey, isUnique, isUniqueConstraint, new IndexColumnMetadataCollection(columns), null, null));
                    }
                }
            }

            var pkColumns = table.PrimaryKeyColumns;

            if (pkColumns.Count == 1 && !results.Any(i => i.IsPrimaryKey)) //need to infer a PK
            {
                results.Add(new IndexMetadata<SQLiteObjectName>(tableName, "(primary key)", true, false, false,
                    new IndexColumnMetadataCollection(new[] { new IndexColumnMetadata(pkColumns.Single(), false, false) }), null, null));
            }

            return new IndexMetadataCollection<SQLiteObjectName>(results);
        }

        /// <summary>
        /// Gets the metadata for a table or view.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override TableOrViewMetadata<SQLiteObjectName, DbType> GetTableOrView(SQLiteObjectName tableName)
        {
            return m_Tables.GetOrAdd(tableName, GetTableOrViewInternal);
        }

        /// <summary>
        /// Returns the table or view derived from the class's name and/or Table attribute.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>TableOrViewMetadata&lt;System.String, DbType&gt;.</returns>
        public override TableOrViewMetadata<SQLiteObjectName, DbType> GetTableOrViewFromClass<TObject>()
        {
            var type = typeof(TObject);
            TableOrViewMetadata<SQLiteObjectName, DbType> result;
            if (m_TypeTableMap.TryGetValue(type, out result))
                return result;

            var typeInfo = MetadataCache.GetMetadata(type);
            if (!string.IsNullOrEmpty(typeInfo.MappedTableName))
            {
                result = GetTableOrView(typeInfo.MappedTableName);
                m_TypeTableMap[type] = result;
                return result;
            }

            //infer table from class name
            result = GetTableOrView(type.Name);
            m_TypeTableMap[type] = result;
            return result;
        }

        /// <summary>
        /// Gets the tables and views that were loaded by this cache.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Call Preload before invoking this method to ensure that all tables and views were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.
        /// </remarks>
        public override IReadOnlyCollection<TableOrViewMetadata<SQLiteObjectName, DbType>> GetTablesAndViews()
        {
            return m_Tables.GetValues();
        }

        /// <summary>
        /// Preloads all of the metadata for this data source.
        /// </summary>
        public override void Preload()
        {
            PreloadTables();
            PreloadViews();
        }

        /// <summary>
        /// Preloads metadata for all database tables.
        /// </summary>
        /// <remarks>This is normally used only for testing. By default, metadata is loaded as needed.</remarks>
        public void PreloadTables()
        {
            const string tableSql =
                @"SELECT
                tbl_name as TableName
                FROM sqlite_master
                WHERE type = 'table'";

            using (var con = new SQLiteConnection(m_ConnectionBuilder.ConnectionString))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(tableSql, con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tableName = reader.GetString(reader.GetOrdinal("TableName"));
                            GetTableOrView(tableName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Preloads metadata for all database views.
        /// </summary>
        /// <remarks>This is normally used only for testing. By default, metadata is loaded as needed.</remarks>
        public void PreloadViews()
        {
            const string viewSql =
                @"SELECT
                tbl_name as ViewName
                FROM sqlite_master
                WHERE type = 'view'";

            using (var con = new SQLiteConnection(m_ConnectionBuilder.ConnectionString))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(viewSql, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var viewName = reader.GetString(reader.GetOrdinal("ViewName"));
                        GetTableOrView(viewName);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the metadata cache, clearing out all cached metadata.
        /// </summary>
        public override void Reset()
        {
            m_Tables.Clear();
            m_TypeTableMap.Clear();
        }

        /// <summary>
        /// Parses the name of the database object.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        protected override SQLiteObjectName ParseObjectName(string name)
        {
            return new SQLiteObjectName(name);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        List<ColumnMetadata<DbType>> GetColumns(SQLiteObjectName tableName, bool isTable)
        {
            /*  NOTE: Should be safe since GetTableOrViewInternal returns null after querying the table name with a
            **  prepared statement, thus proving that the table name exists.
            */
            var hasPrimarykey = false;
            var columnSql = $"PRAGMA table_info('{tableName.Name}')";

            var columns = new List<ColumnMetadata<DbType>>();
            using (var con = new SQLiteConnection(m_ConnectionBuilder.ConnectionString))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(columnSql, con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString(reader.GetOrdinal("name"));
                            var typeName = reader.GetString(reader.GetOrdinal("type"));
                            var isPrimaryKey = reader.GetInt32(reader.GetOrdinal("pk")) != 0 ? true : false;
                            var isnNullable = !reader.GetBoolean(reader.GetOrdinal("notnull"));
                            hasPrimarykey = hasPrimarykey || isPrimaryKey;

                            columns.Add(new ColumnMetadata<DbType>(name, false, isPrimaryKey, false, typeName, null, "[" + name + "]", isnNullable, null, null, null, null));
                        }
                    }
                }
            }

            //Tables wihtout a primary key always have a ROWID.
            //We can't tell if other tables have one or not.
            if (isTable && !hasPrimarykey)
                columns.Add(new ColumnMetadata<DbType>("ROWID", true, false, true, "INTEGER", null, "[ROWID]", false, null, null, null, null));

            return columns;
        }

        private TableOrViewMetadata<SQLiteObjectName, DbType> GetTableOrViewInternal(SQLiteObjectName tableName)
        {
            const string tableSql =
                @"SELECT
                type AS ObjectType,
                tbl_name AS ObjectName
                FROM sqlite_master
                WHERE UPPER(tbl_name) = UPPER(@Name) AND
                      (type='table' OR type='view')";

            string actualName;
            bool isTable;

            using (var con = new SQLiteConnection(m_ConnectionBuilder.ConnectionString))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(tableSql, con))
                {
                    cmd.Parameters.AddWithValue("@Name", tableName.Name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new MissingObjectException($"Could not find table or view {tableName}");

                        actualName = reader.GetString(reader.GetOrdinal("ObjectName"));
                        var objectType = reader.GetString(reader.GetOrdinal("ObjectType"));
                        isTable = objectType.Equals("table");
                    }
                }
            }

            var columns = GetColumns(tableName, isTable);
            return new TableOrViewMetadata<SQLiteObjectName, DbType>(this, actualName, isTable, columns);
        }
    }
}
