using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using Tortuga.Anchor;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.SQLite;

/// <summary>
/// Handles caching of metadata for various SQLite tables and views.
/// </summary>
public sealed class SQLiteMetadataCache : DbDatabaseMetadataCache<SQLiteObjectName>
{
	readonly SQLiteConnectionStringBuilder m_ConnectionBuilder;
	readonly ConcurrentDictionary<SQLiteObjectName, TableOrViewMetadata<SQLiteObjectName, DbType>> m_Tables = new();

	readonly ConcurrentDictionary<Type, TableOrViewMetadata<SQLiteObjectName, DbType>> m_TypeTableMap = new();

	/// <summary>
	/// Creates a new instance of <see cref="SQLiteMetadataCache"/>
	/// </summary>
	/// <param name="connectionBuilder">The connection builder.</param>
	public SQLiteMetadataCache(SQLiteConnectionStringBuilder connectionBuilder)
	{
		m_ConnectionBuilder = connectionBuilder;
	}

	/// <summary>
	/// Gets the maximum number of parameters in a single SQL batch.
	/// </summary>
	/// <value>The maximum number of parameters.</value>
	/// <remarks>https://sqlite.org/limits.html</remarks>
	public override int? MaxParameters => 999;

	public string ClrTypeToSqlTypeName<T>(bool? isNullable = null, int? maxLength = null, int? precision = null, int? scale = null)
	{
		var nullText = (isNullable ?? true) ? " NULL" : " NOT NULL";

		var type = typeof(T);
		if (type == typeof(string)) return "TEXT" + nullText;
		if (type == typeof(char)) return "TEXT NOT NULL";
		if (type == typeof(char?)) return "TEXT NULL";
		if (type == typeof(short)) return "INTEGER NOT NULL";
		if (type == typeof(short?)) return "INTEGER NULL";
		if (type == typeof(int)) return "INTEGER NOT NULL";
		if (type == typeof(int?)) return "INTEGER NULL";
		if (type == typeof(long)) return "INTEGER NOT NULL";
		if (type == typeof(long?)) return "INTEGER NULL";
		if (type == typeof(ushort)) return "INTEGER NOT NULL";
		if (type == typeof(ushort?)) return "INTEGER NULL";
		if (type == typeof(uint)) return "INTEGER NOT NULL";
		if (type == typeof(uint?)) return "INTEGER NULL";
		if (type == typeof(ulong)) return "INTEGER NOT NULL";
		if (type == typeof(ulong?)) return "INTEGER NULL";
		if (type == typeof(float)) return "REAL NOT NULL";
		if (type == typeof(float?)) return "REAL NULL";
		if (type == typeof(double)) return "REAL NOT NULL";
		if (type == typeof(double?)) return "REAL NULL";
		if (type == typeof(decimal)) return "REAL NOT NULL";
		if (type == typeof(decimal?)) return "REAL NULL";
		if (type == typeof(Guid)) return "TEXT NOT NULL";
		if (type == typeof(Guid?)) return "TEXT NULL";
#if NET6_0_OR_GREATER
		if (type == typeof(DateOnly)) return "TEXT NOT NULL";
		if (type == typeof(DateOnly?)) return "TEXT NULL";
		if (type == typeof(TimeOnly)) return "TEXT NOT NULL";
		if (type == typeof(TimeOnly?)) return "TEXT NULL";
#endif
		if (type == typeof(DateTime)) return "TEXT NOT NULL";
		if (type == typeof(DateTime?)) return "TEXT NULL";
		if (type == typeof(TimeSpan)) return "TEXT NOT NULL";
		if (type == typeof(TimeSpan?)) return "TEXT NULL";
		if (type == typeof(DateTimeOffset)) return "TEXT NOT NULL";
		if (type == typeof(DateTimeOffset?)) return "TEXT NULL";
		throw new ArgumentOutOfRangeException($"Cannot map {type.Name} to a SQLite type");
	}

	/// <summary>
	/// Gets the indexes for a table.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <returns></returns>
	/// <remarks>
	/// This should be cached on a TableOrViewMetadata object.
	/// </remarks>
	public override IndexMetadataCollection<SQLiteObjectName, DbType> GetIndexesForTable(SQLiteObjectName tableName)
	{
		var table = GetTableOrView(tableName);

		var indexSql = $"PRAGMA index_list('{tableName.Name}')";
		var results = new List<IndexMetadata<SQLiteObjectName, DbType>>();
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
					var name = reader.GetString("name");
					var isUnique = reader.GetInt64("unique") != 0;
					var origin = reader.GetString("origin");
					var isPrimaryKey = string.Equals(origin, "pk", StringComparison.Ordinal);
					var isUniqueConstraint = string.Equals(origin, "u", StringComparison.Ordinal);

					var columns = new List<IndexColumnMetadata<DbType>>();

					using (var cmd2 = new SQLiteCommand($"PRAGMA index_xinfo('{name}')", con2))
					using (var reader2 = cmd2.ExecuteReader())
					{
						while (reader2.Read())
						{
							var colName = reader2.GetStringOrNull("name");
							var isIncluded = reader2.GetInt64("key") == 0;
							var isDescending = isIncluded ? (bool?)null : reader2.GetInt64("desc") != 0;

							ColumnMetadata<DbType>? column;
							if (colName != null)
								column = table.Columns.Single(c => string.Equals(c.SqlName, colName, StringComparison.Ordinal));
							else //a null column name is really the ROWID
							{
								column = table.Columns.SingleOrDefault(c => string.Equals(c.SqlName, "ROWID", StringComparison.Ordinal));

								if (column == null) //The ROWID may be aliased as the primary key
									column = table.PrimaryKeyColumns.Single();
							}

							columns.Add(new IndexColumnMetadata<DbType>(column, isDescending, isIncluded));
						}
					}

					var indexType = isPrimaryKey ? IndexType.Clustered : IndexType.Nonclustered;

					results.Add(new IndexMetadata<SQLiteObjectName, DbType>(tableName, name, isPrimaryKey, isUnique, isUniqueConstraint, new IndexColumnMetadataCollection<DbType>(columns), null, null, indexType));
				}
			}
		}

		var pkColumns = table.PrimaryKeyColumns;

		if (pkColumns.Count == 1 && !results.Any(i => i.IsPrimaryKey)) //need to infer a PK
		{
			results.Add(new IndexMetadata<SQLiteObjectName, DbType>(tableName, "(primary key)", true, false, false,
				new IndexColumnMetadataCollection<DbType>(new[] { new IndexColumnMetadata<DbType>(pkColumns.Single(), false, false) }), null, null, IndexType.Clustered));
		}

		return new IndexMetadataCollection<SQLiteObjectName, DbType>(results);
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
						var tableName = reader.GetString("TableName");
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
					var viewName = reader.GetString("ViewName");
					GetTableOrView(viewName);
				}
			}
		}
	}

	/// <summary>
	/// Quotes the name of the column.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <returns>System.String.</returns>
	/// <exception cref="System.ArgumentException">columnName</exception>
	/// <remarks>This assumes the column name wasn't already quoted.</remarks>
	public override string QuoteColumnName(string columnName)
	{
		if (string.IsNullOrEmpty(columnName))
			throw new ArgumentException($"{nameof(columnName)} is null or empty.", nameof(columnName));

		if (columnName == "*" || columnName[0] == '[')
			return columnName;

		return "[" + columnName + "]";
	}

	/// <summary>
	/// Parse a string and return the database specific representation of the database object's name as a quoted string.
	/// </summary>
	/// <param name="name">Name of the object.</param>
	/// <returns>System.String.</returns>
	public override string QuoteObjectName(string name) => ParseObjectName(name).ToQuotedString();

	/// <summary>
	/// Resets the metadata cache, clearing out all cached metadata.
	/// </summary>
	public override void Reset()
	{
		m_Tables.Clear();
		m_TypeTableMap.Clear();
	}

	/// <summary>
	/// Parse a string and return the database specific representation of the object name.
	/// </summary>
	/// <param name="schema">The schema.</param>
	/// <param name="name">The name.</param>
	/// <returns>SQLiteObjectName.</returns>
	/// <exception cref="NotImplementedException"></exception>
	protected override SQLiteObjectName ParseObjectName(string? schema, string name) => new SQLiteObjectName(name);

	/// <summary>
	/// Determines the database column type from the column type name.
	/// </summary>
	/// <param name="typeName">Name of the database column type.</param>
	/// <param name="isUnsigned">NOT USED</param>
	/// <returns></returns>
	/// <remarks>This does not honor registered types. This is only used for the database's hard-coded list of native types.</remarks>
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	protected override DbType? SqlTypeNameToDbType(string? typeName, bool? isUnsigned = null)
	{
		if (typeName.IsNullOrEmpty())
			return null;

		var cleanTypeName = typeName.ToUpperInvariant();
		if (cleanTypeName.IndexOf("(", StringComparison.OrdinalIgnoreCase) >= 0)
			cleanTypeName = cleanTypeName.Substring(0, cleanTypeName.IndexOf("(", StringComparison.OrdinalIgnoreCase));

		return cleanTypeName switch
		{
			"BIGINT" => DbType.Int64,
			"BIT" => DbType.Boolean,
			"BLOB" => DbType.Binary,
			"BOOLEAN" => DbType.Boolean,
			"CHAR" => DbType.AnsiString,
			"CHARACTER" => DbType.AnsiString,
			"CLOB" => DbType.String,
			"DATE" => DbType.Date,
			"DATETIME" => DbType.DateTime,
			"DATETIME2" => DbType.DateTime2,
			"DATETIMEOFFSET" => DbType.DateTimeOffset,
			"DECIMAL" => DbType.Decimal,
			"DOUBLE PRECISION" => DbType.Double,
			"DOUBLE" => DbType.Double,
			"FLOAT" => DbType.Single,
			"INT" => DbType.Int64,
			"INT2" => DbType.Int16,
			"INT8" => DbType.Int64,
			"INTEGER" => DbType.Int64,
			"MEDIUMINT" => DbType.Int32,
			"NATIVE CHARACTER" => DbType.String,
			"NCHAR" => DbType.String,
			"NUMERIC" => DbType.Decimal,
			"NVARCHAR" => DbType.String,
			"REAL" => DbType.Single,
			"SMALLINT" => DbType.Int16,
			"TEXT" => DbType.AnsiString,
			"TIME" => DbType.Time,
			"TINYINT" => DbType.SByte,
			"UNSIGNED BIG INT" => DbType.UInt64,
			"VARCHAR" => DbType.AnsiString,
			"VARYING CHARACTER" => DbType.AnsiString,
			_ => null,
		};
	}

	[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
	ColumnMetadataCollection<DbType> GetColumns(SQLiteObjectName tableName, bool isTable)
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
						var name = reader.GetString("name");
						var typeName = reader.GetString("type");
						var isPrimaryKey = reader.GetInt32("pk") != 0 ? true : false;
						var isnNullable = !reader.GetBoolean("notnull");
						hasPrimarykey = hasPrimarykey || isPrimaryKey;
						string fullTypeName = ""; //Task-292: Add support for full name
						columns.Add(new ColumnMetadata<DbType>(name, false, isPrimaryKey, false, typeName, SqlTypeNameToDbType(typeName), QuoteColumnName(name), isnNullable, null, null, null, fullTypeName, ToClrType(typeName, isnNullable, null)));
					}
				}
			}
		}

		//Tables without a primary key always have a ROWID.
		//We can't tell if other tables have one or not.
		if (isTable && !hasPrimarykey)
			columns.Add(new ColumnMetadata<DbType>("ROWID", true, false, true, "INTEGER", DbType.Int64, "[ROWID]", false, null, null, null, "", typeof(long)));

		return new ColumnMetadataCollection<DbType>(tableName.ToString(), columns);
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

					actualName = reader.GetString("ObjectName");
					var objectType = reader.GetString("ObjectType");
					isTable = objectType.Equals("table", StringComparison.Ordinal);
				}
			}
		}

		var columns = GetColumns(tableName, isTable);
		return new TableOrViewMetadata<SQLiteObjectName, DbType>(this, actualName, isTable, columns);
	}
}
