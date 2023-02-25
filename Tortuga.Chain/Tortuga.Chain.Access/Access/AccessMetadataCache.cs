using System.Collections.Concurrent;
using System.Data.OleDb;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Tortuga.Anchor;
using Tortuga.Chain.Aggregates;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.Access;

/// <summary>
/// Handles caching of metadata for various Access tables and views.
/// </summary>
public sealed class AccessMetadataCache : OleDbDatabaseMetadataCache<AccessObjectName>
{
	readonly OleDbConnectionStringBuilder m_ConnectionBuilder;
	readonly ConcurrentDictionary<AccessObjectName, TableOrViewMetadata<AccessObjectName, OleDbType>> m_Tables = new();
	readonly ConcurrentDictionary<Type, TableOrViewMetadata<AccessObjectName, OleDbType>> m_TypeTableMap = new();

	ConcurrentDictionary<Guid, DataTable> m_DataTableCache = new();
	bool m_SchemaLoaded;

	/// <summary>
	/// Creates a new instance of <see cref="AccessMetadataCache"/>
	/// </summary>
	/// <param name="connectionBuilder">The connection builder.</param>
	public AccessMetadataCache(OleDbConnectionStringBuilder connectionBuilder)
	{
		m_ConnectionBuilder = connectionBuilder;
	}

	/// <summary>
	/// Gets the maximum number of parameters in a single SQL batch.
	/// </summary>
	/// <value>The maximum number of parameters.</value>
	/// <remarks>https://stackoverflow.com/a/54149292/5274</remarks>
	public override int? MaxParameters => 768;

	/// <summary>
	/// Gets an aggregate function.
	/// </summary>
	/// <param name="aggregateType">Type of the aggregate.</param>
	/// <param name="columnName">Name of the column to insert into the function.</param>
	/// <returns>A string suitable for use in an aggregate.</returns>
	/// <exception cref="System.NotSupportedException">Access does not support distinct counts.</exception>
	/// <exception cref="System.NotSupportedException">Access does not support distinct sums.</exception>
	public override string GetAggregateFunction(AggregateType aggregateType, string columnName)
	{
		if (aggregateType == AggregateType.CountDistinct)
			throw new NotSupportedException("Access does not support distinct counts.");
		if (aggregateType == AggregateType.SumDistinct)
			throw new NotSupportedException("Access does not support distinct sums.");

		return base.GetAggregateFunction(aggregateType, columnName);
	}

	/// <summary>
	/// Gets the indexes for a table.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <returns></returns>
	/// <remarks>
	/// This should be cached on a TableOrViewMetadata object.
	/// </remarks>
	public override IndexMetadataCollection<AccessObjectName, OleDbType> GetIndexesForTable(AccessObjectName tableName)
	{
		var result = new List<IndexMetadata<AccessObjectName, OleDbType>>();
		var indexDT = GetSchemaTable(OleDbSchemaGuid.Indexes);

		var allColumns = GetTableOrView(tableName).Columns;
		var indexes = indexDT.AsEnumerable().Where(r => string.Equals(r["TABLE_NAME"].ToString(), tableName.Name, StringComparison.Ordinal)).GroupBy(r => r["INDEX_NAME"].ToString()).ToList();

		foreach (var index in indexes)
		{
			var name = index.Key;
			var unique = (bool)index.First()["UNIQUE"];
			var isPrimary = (bool)index.First()["PRIMARY_KEY"];

			var columns = new IndexColumnMetadata<OleDbType>[index.Count()];
			foreach (var column in index)
			{
				var details = allColumns[(string)column["COLUMN_NAME"]];
				columns[(int)(long)column["ORDINAL_POSITION"] - 1] = new IndexColumnMetadata<OleDbType>(details, false, false);
			}

			var indexType = isPrimary ? IndexType.Clustered : IndexType.Nonclustered;

			result.Add(new IndexMetadata<AccessObjectName, OleDbType>(tableName, name, isPrimary, unique, false, new IndexColumnMetadataCollection<OleDbType>(columns), null, null, indexType));
		}

		return new IndexMetadataCollection<AccessObjectName, OleDbType>(result);
	}

	/// <summary>
	/// Gets the metadata for a table or view.
	/// </summary>
	/// <param name="tableName"></param>
	/// <returns></returns>
	public override TableOrViewMetadata<AccessObjectName, OleDbType> GetTableOrView(AccessObjectName tableName)
	{
		if (!m_SchemaLoaded)
			Preload();

		TableOrViewMetadata<AccessObjectName, OleDbType>? result;
		if (m_Tables.TryGetValue(tableName, out result))
			return result;

		throw new MissingObjectException($"Could not find table or view {tableName}");
	}

	/// <summary>
	/// Gets the tables and views that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// Call Preload before invoking this method to ensure that all tables and views were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.
	/// </remarks>
	public override IReadOnlyCollection<TableOrViewMetadata<AccessObjectName, OleDbType>> GetTablesAndViews() => m_Tables.GetValues();

	/// <summary>
	/// Preloads all of the metadata for this data source.
	/// </summary>
	public override void Preload()
	{
		var columns = GetColumnsDataTable();
		var primaryKeys = GetPrimaryKeysDataTable();

		PreloadTables(columns, primaryKeys);
		PreloadViews(columns);

		m_SchemaLoaded = true;
	}

	/// <summary>
	/// Preloads metadata for all database tables.
	/// </summary>
	/// <remarks>This is normally used only for testing. By default, metadata is loaded as needed.</remarks>
	public void PreloadTables() => PreloadTables(GetColumnsDataTable(), GetPrimaryKeysDataTable());

	/// <summary>
	/// Preloads metadata for all database views.
	/// </summary>
	/// <remarks>This is normally used only for testing. By default, metadata is loaded as needed.</remarks>
	public void PreloadViews() => PreloadViews(GetColumnsDataTable());

	/// <summary>
	/// Quotes the name of the column.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <exception cref="System.ArgumentException">columnName</exception>
	/// <remarks>This assumes the column name wasn't already quoted.</remarks>
	public override string QuoteColumnName(string columnName)
	{
		if (string.IsNullOrEmpty(columnName))
			throw new ArgumentException($"{nameof(columnName)} is null or empty.", nameof(columnName));

		if (columnName == "*" || columnName[0] == '[')
			return columnName;

		return $"[{columnName}]";
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
		m_DataTableCache.Clear();
		m_Tables.Clear();
		m_TypeTableMap.Clear();
		m_SchemaLoaded = false;
	}

	/// <summary>
	/// Parse a string and return the database specific representation of the object name.
	/// </summary>
	/// <param name="schema">The schema.</param>
	/// <param name="name">The name.</param>
	/// <returns>AccessObjectName.</returns>
	protected override AccessObjectName ParseObjectName(string? schema, string name) => name;

	/// <summary>
	/// Determines the database column type from the column type name.
	/// </summary>
	/// <param name="typeName">Name of the database column type.</param>
	/// <param name="isUnsigned">NOT USED</param>
	/// <returns></returns>
	/// <remarks>This does not honor registered types. This is only used for the database's hard-coded list of native types.</remarks>
	protected override OleDbType? SqlTypeNameToDbType(string typeName, bool? isUnsigned = null)
	{
		if (Enum.TryParse<OleDbType>(typeName, out var result))
			return result;

		return null;
	}

	[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
	ColumnMetadataCollection<OleDbType> GetColumns(string tableName, DataTable columns, DataTable? primaryKeys)
	{
		var result = new List<ColumnMetadata<OleDbType>>();
		DataTable tableSchema;
		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			using (var adapter = new OleDbDataAdapter($"SELECT * FROM [{tableName}] WHERE 1=0", con))
				tableSchema = adapter.FillSchema(new DataTable() { Locale = CultureInfo.InvariantCulture }, SchemaType.Source)!;
		}

		foreach (DataColumn col in tableSchema.Columns)
		{
			var name = col.ColumnName;
			var isPrimaryKey = false;
			var isIdentity = col.AutoIncrement;
			OleDbType? type = null;

			if (primaryKeys != null)
				foreach (DataRow row in primaryKeys.Rows)
				{
					if (string.Equals(row["TABLE_NAME"].ToString(), tableName, StringComparison.Ordinal) && string.Equals(row["COLUMN_NAME"].ToString(), name, StringComparison.Ordinal))
					{
						isPrimaryKey = true;
						break;
					}
				}

			bool? isNullable = null;
			long? maxLength = null;
			int? precision = null;
			int? scale = null;
			string typeName = "";
			string fullTypeName = ""; //Task-290: Add support for full name

			foreach (DataRow row in columns.Rows)
			{
				if (string.Equals(row["TABLE_NAME"].ToString(), tableName, StringComparison.Ordinal) && string.Equals(row["COLUMN_NAME"].ToString(), name, StringComparison.Ordinal))
				{
					type = (OleDbType)row["DATA_TYPE"];
					isNullable = (bool)row["IS_NULLABLE"];
					precision = row["NUMERIC_PRECISION"] != DBNull.Value ? (int?)row["NUMERIC_PRECISION"] : null;
					scale = row["NUMERIC_SCALE"] != DBNull.Value ? (int?)row["NUMERIC_SCALE"] : null;
					maxLength = row["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? (long?)row["CHARACTER_MAXIMUM_LENGTH"] : null;
					break;
				}
			}

			Type? clrType = null;
			if (type.HasValue)
			{
				typeName = type.Value.ToString();
				clrType = ToClrType(type.Value, isNullable ?? true, (int?)maxLength);
			}

			result.Add(new ColumnMetadata<OleDbType>(name, false, isPrimaryKey, isIdentity, typeName, type ?? OleDbType.Empty, QuoteColumnName(name), isNullable, (int?)maxLength, precision, scale, fullTypeName, clrType));
		}

		return new ColumnMetadataCollection<OleDbType>(tableName, result);
	}

	DataTable GetColumnsDataTable() => GetSchemaTable(OleDbSchemaGuid.Columns);

	DataTable GetPrimaryKeysDataTable() => GetSchemaTable(OleDbSchemaGuid.Primary_Keys);

	DataTable GetSchemaTable(Guid oleDbSchemaGuid)
	{
		return m_DataTableCache.GetOrAdd(oleDbSchemaGuid, sg =>
		{
			using (var connection = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
			{
				connection.Open();
				return connection.GetOleDbSchemaTable(sg, null)!;
			}
		});
	}

	void PreloadTables(DataTable columnsDataTable, DataTable primaryKeys)
	{
		using (var connection = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			connection.Open();
			var dtTables = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null)!;
			foreach (DataRow row in dtTables.Rows)
			{
				if (row["TABLE_TYPE"].ToString() != "TABLE")
					continue;

				var name = row["TABLE_NAME"].ToString()!;
				var columns = GetColumns(name, columnsDataTable, primaryKeys);
				m_Tables[name] = new TableOrViewMetadata<AccessObjectName, OleDbType>(this, name, true, columns);
			}
		}
	}

	void PreloadViews(DataTable columnsDataTable)
	{
		using (var connection = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			connection.Open();
			var dtViews = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Views, null)!;
			foreach (DataRow row in dtViews.Rows)
			{
				var name = row["TABLE_NAME"].ToString()!;
				var columns = GetColumns(name, columnsDataTable, null);
				m_Tables[name] = new TableOrViewMetadata<AccessObjectName, OleDbType>(this, name, false, columns);
			}
		}
	}
}
