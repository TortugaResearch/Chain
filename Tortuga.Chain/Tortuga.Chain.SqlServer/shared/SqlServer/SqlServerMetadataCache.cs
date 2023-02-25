using System.Diagnostics.CodeAnalysis;
using Tortuga.Chain.Aggregates;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.SqlServer;

/// <summary>
/// Class SqlServerMetadataCache.
/// </summary>
public sealed partial class SqlServerMetadataCache
{
	internal readonly SqlConnectionStringBuilder m_MasterConnectionBuilder;

	/// <summary>
	/// Initializes a new instance of the <see cref="SqlServerMetadataCache"/> class.
	/// </summary>
	/// <param name="connectionBuilder">The connection builder.</param>
	public SqlServerMetadataCache(SqlConnectionStringBuilder connectionBuilder)
	{
		if (connectionBuilder == null)
			throw new ArgumentNullException(nameof(connectionBuilder), $"{nameof(connectionBuilder)} is null");

		m_ConnectionBuilder = connectionBuilder;

		m_MasterConnectionBuilder = new SqlConnectionStringBuilder(connectionBuilder.ConnectionString) { InitialCatalog = "master" };
	}

	/// <summary>
	/// Returns the current database.
	/// </summary>
	/// <returns></returns>
	public string DatabaseName
	{
		get
		{
			if (m_DatabaseName == null)
			{
				using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
				{
					con.Open();
					using (var cmd = new SqlCommand("SELECT DB_NAME () AS DatabaseName", con))
					{
						m_DatabaseName = (string)cmd.ExecuteScalar();
					}
				}
			}
			return m_DatabaseName;
		}
	}

	/// <summary>
	/// Returns the user's default schema.
	/// </summary>
	/// <returns></returns>
	public string DefaultSchema
	{
		get
		{
			if (m_DefaultSchema == null)
			{
				using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
				{
					con.Open();
					using (var cmd = new SqlCommand("SELECT SCHEMA_NAME () AS DefaultSchema", con))
					{
						m_DefaultSchema = (string)cmd.ExecuteScalar();
					}
				}
			}
			return m_DefaultSchema;
		}
	}

	/// <summary>
	/// This is used to decide which option overrides to set when establishing a connection.
	/// </summary>
	internal SqlServerEffectiveSettings? ServerDefaultSettings { get; set; }

	/// <summary>
	/// Gets an aggregate function.
	/// </summary>
	/// <param name="aggregateType">Type of the aggregate.</param>
	/// <param name="columnName">Name of the column to insert into the function.</param>
	/// <returns>A string suitable for use in an aggregate.</returns>
	public override string GetAggregateFunction(AggregateType aggregateType, string columnName)
	{
		return aggregateType switch
		{
			AggregateType.Count => $"COUNT_BIG({QuoteColumnName(columnName!)})",
			AggregateType.CountDistinct => $"COUNT_BIG(DISTINCT {QuoteColumnName(columnName!)})",
			_ => base.GetAggregateFunction(aggregateType, columnName),
		};
	}

	/// <summary>
	/// Gets the indexes for a table.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <returns></returns>
	/// <remarks>
	/// This should be cached on a TableOrViewMetadata object.
	/// </remarks>
	public override IndexMetadataCollection<SqlServerObjectName, SqlDbType> GetIndexesForTable(SqlServerObjectName tableName)
	{
		const string indexSql = @"SELECT i.name,
	   i.is_primary_key,
	   i.is_unique,
	   i.is_unique_constraint,
	   i.index_id,
	   i.type,
	   (SELECT SUM(used_page_count) * 8 FROM sys.dm_db_partition_stats ddps WHERE ddps.object_id=i.object_id AND ddps.index_id = i.index_id) AS IndexSizeKB,
	   (SELECT SUM(row_count) FROM sys.dm_db_partition_stats ddps WHERE ddps.object_id=i.object_id AND ddps.index_id = i.index_id) AS [RowCount]
FROM sys.indexes i
	INNER JOIN sys.objects o
		ON i.object_id = o.object_id
	INNER JOIN sys.schemas s
		ON o.schema_id = s.schema_id
WHERE o.name = @Name
	  AND s.name = @Schema;";

		var allColumns = GetColumnsForIndex(tableName);

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();

			using (var cmd = new SqlCommand(indexSql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", tableName.Schema);
				cmd.Parameters.AddWithValue("@Name", tableName.Name);

				var results = new List<IndexMetadata<SqlServerObjectName, SqlDbType>>();

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var is_primary_key = reader.GetBoolean("is_primary_key");
						var is_unique = reader.GetBoolean("is_unique");
						var is_unique_constraint = reader.GetBoolean("is_unique_constraint");
						var index_id = reader.GetInt32("index_id");
						var name = reader.GetStringOrNull("Name");
						var columns = new IndexColumnMetadataCollection<SqlDbType>(allColumns.Where(c => c.IndexId == index_id));
						var indexSize = reader.GetInt64("IndexSizeKB");
						var rowCount = reader.GetInt64("RowCount");

						IndexType indexType;
						switch (reader.GetByte("type"))
						{
							case 0: indexType = IndexType.Heap; break;
							case 1: indexType = IndexType.Clustered; break;
							case 2: indexType = IndexType.Nonclustered; break;
							case 3: indexType = IndexType.Xml; break;
							case 4: indexType = IndexType.Spatial; break;
							case 5: indexType = IndexType.ClusteredColumnstoreIndex; break;
							case 6: indexType = IndexType.NonclusteredColumnstoreIndex; break;
							case 7: indexType = IndexType.NonclusteredHashIndex; break;
							default: indexType = IndexType.Unknown; break;
						}

						results.Add(new IndexMetadata<SqlServerObjectName, SqlDbType>(tableName, name, is_primary_key, is_unique, is_unique_constraint, columns, indexSize, rowCount, indexType));
					}

					return new IndexMetadataCollection<SqlServerObjectName, SqlDbType>(results);
				}
			}
		}
	}

	/// <summary>
	/// Gets the detailed metadata for a table or view.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <returns>SqlServerTableOrViewMetadata&lt;TDbType&gt;.</returns>
	public new SqlServerTableOrViewMetadata<SqlDbType> GetTableOrView(SqlServerObjectName tableName)
	{
		return m_Tables.GetOrAdd(tableName, GetTableOrViewInternal);
	}

	/// <summary>
	/// Preloads the scalar functions.
	/// </summary>
	public void PreloadScalarFunctions()
	{
		const string TvfSql =
			@"SELECT
				s.name AS SchemaName,
				o.name AS Name,
				o.object_id AS ObjectId
				FROM sys.objects o
				INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
				WHERE o.type in ('FN')";

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(TvfSql, con))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schema = reader.GetString("SchemaName");
						var name = reader.GetString("Name");
						GetScalarFunction(new SqlServerObjectName(schema, name));
					}
				}
			}
		}
	}

	/// <summary>
	/// Preloads the stored procedures.
	/// </summary>
	public void PreloadStoredProcedures()
	{
		const string StoredProcedureSql =
		@"SELECT
				s.name AS SchemaName,
				sp.name AS Name
				FROM SYS.procedures sp
				INNER JOIN sys.schemas s ON sp.schema_id = s.schema_id;";

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(StoredProcedureSql, con))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schema = reader.GetString("SchemaName");
						var name = reader.GetString("Name");
						GetStoredProcedure(new SqlServerObjectName(schema, name));
					}
				}
			}
		}
	}

	/// <summary>
	/// Preloads the table value functions.
	/// </summary>
	public void PreloadTableFunctions()
	{
		const string TvfSql =
			@"SELECT
				s.name AS SchemaName,
				o.name AS Name,
				o.object_id AS ObjectId
				FROM sys.objects o
				INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
				WHERE o.type in ('TF', 'IF', 'FT')";

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(TvfSql, con))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schema = reader.GetString("SchemaName");
						var name = reader.GetString("Name");
						GetTableFunction(new SqlServerObjectName(schema, name));
					}
				}
			}
		}
	}

	/// <summary>
	/// Preloads metadata for all tables.
	/// </summary>
	/// <remarks>This is normally used only for testing. By default, metadata is loaded as needed.</remarks>
	public void PreloadTables()
	{
		const string tableList = "SELECT t.name AS Name, s.name AS SchemaName FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id=s.schema_id ORDER BY s.name, t.name";

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(tableList, con))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schema = reader.GetString("SchemaName");
						var name = reader.GetString("Name");
						GetTableOrView(new SqlServerObjectName(schema, name));
					}
				}
			}
		}
	}

	/// <summary>
	/// Preloads the user defined types.
	/// </summary>
	/// <remarks>This is normally used only for testing. By default, metadata is loaded as needed.</remarks>
	public void PreloadUserDefinedTableTypes()
	{
		const string tableList = @"SELECT s.name AS SchemaName, t.name AS Name FROM sys.types t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE	t.is_user_defined = 1 AND t.is_table_type = 1;";

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(tableList, con))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schema = reader.GetString("SchemaName");
						var name = reader.GetString("Name");
						GetUserDefinedTableType(new SqlServerObjectName(schema, name));
					}
				}
			}
		}
	}

	/// <summary>
	/// Preloads metadata for all views.
	/// </summary>
	/// <remarks>This is normally used only for testing. By default, metadata is loaded as needed.</remarks>
	public void PreloadViews()
	{
		const string tableList = "SELECT t.name AS Name, s.name AS SchemaName FROM sys.views t INNER JOIN sys.schemas s ON t.schema_id=s.schema_id ORDER BY s.name, t.name";

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(tableList, con))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schema = reader.GetString("SchemaName");
						var name = reader.GetString("Name");
						GetTableOrView(new SqlServerObjectName(schema, name));
					}
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

		return $"[{columnName}]";
	}

	/// <summary>
	/// Parse a string and return the database specific representation of the database object's name as a quoted string.
	/// </summary>
	/// <param name="name">Name of the object.</param>
	/// <returns>System.String.</returns>
	public override string QuoteObjectName(string name) => ParseObjectName(name).ToQuotedString();

	/// <summary>
	/// Converts a value to a string suitable for use in a SQL statement.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="dbType">Optional database column type.</param>
	/// <returns></returns>
	public override string ValueToSqlValue(object value, SqlDbType? dbType)
	{
		switch (value)
		{
			case string s:
				{
					switch (dbType)
					{
						case SqlDbType.Char:
						case SqlDbType.VarChar:
						case SqlDbType.Text:
							return "'" + s.Replace("'", "''", StringComparison.OrdinalIgnoreCase) + "'";

						case SqlDbType.NChar:
						case SqlDbType.NVarChar:
						case SqlDbType.NText:
							return "N'" + s.Replace("'", "''", StringComparison.OrdinalIgnoreCase) + "'";

						default: //Assume Unicode
							return "N'" + s.Replace("'", "''", StringComparison.OrdinalIgnoreCase) + "'";
					}
				}

			default:
				return base.ValueToSqlValue(value, dbType);
		}
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is designed to fail silently.")]
	internal StoredProcedureMetadata<SqlServerObjectName, SqlDbType>? GetMasterStoredProcedure(SqlServerObjectName procedureName)
	{
		try
		{
			const string StoredProcedureSql =
				@"SELECT
					s.name AS SchemaName,
					sp.name AS Name,
					sp.object_id AS ObjectId
					FROM SYS.procedures sp
					INNER JOIN sys.schemas s ON sp.schema_id = s.schema_id
					WHERE s.name = @Schema AND sp.Name = @Name";

			string actualSchema;
			string actualName;
			int objectId;

			using (var con = new SqlConnection(m_MasterConnectionBuilder.ConnectionString))
			{
				con.Open();
				using (var cmd = new SqlCommand(StoredProcedureSql, con))
				{
					cmd.Parameters.AddWithValue("@Schema", procedureName.Schema ?? "dbo");
					cmd.Parameters.AddWithValue("@Name", procedureName.Name);
					using (var reader = cmd.ExecuteReader())
					{
						if (!reader.Read())
							return null;

						actualSchema = reader.GetString("SchemaName");
						actualName = reader.GetString("Name");
						objectId = reader.GetInt32("ObjectId");
					}
				}
				var objectName = new SqlServerObjectName(actualSchema, actualName);
				var parameters = GetParameters(objectName.ToString(), objectId, con);

				return new StoredProcedureMetadata<SqlServerObjectName, SqlDbType>(objectName, parameters);
			}
		}
		catch
		{
			return null;
		}
	}

	internal ScalarFunctionMetadata<SqlServerObjectName, SqlDbType> GetScalarFunctionInternal(SqlServerObjectName scalarFunctionName)
	{
		const string sql =
	@"SELECT	s.name AS SchemaName,
		o.name AS Name,
		o.object_id AS ObjectId,
		COALESCE(t.name, t2.name) AS TypeName,
		p.is_nullable,
		CONVERT(INT, COALESCE(p.max_length, t.max_length, t2.max_length)) AS max_length,
		CONVERT(INT, COALESCE(p.precision, t.precision, t2.precision)) AS precision,
		CONVERT(INT, COALESCE(p.scale, t.scale, t2.scale)) AS scale

		FROM	sys.objects o
		INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
		INNER JOIN sys.parameters p ON p.object_id = o.object_id AND p.parameter_id = 0
		LEFT JOIN sys.types t on p.system_type_id = t.user_type_id
		LEFT JOIN sys.types t2 ON p.user_type_id = t2.user_type_id
		WHERE	o.type IN ('FN')
		AND s.name = @Schema
		AND o.name = @Name;";

		string actualSchema;
		string actualName;
		int objectId;

		string fullTypeName;
		string typeName;
		bool isNullable;
		int? maxLength;
		int? precision;
		int? scale;

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(sql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", scalarFunctionName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name", scalarFunctionName.Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.Read())
						throw new MissingObjectException($"Could not find scalar function {scalarFunctionName}");
					actualSchema = reader.GetString("SchemaName");
					actualName = reader.GetString("Name");
					objectId = reader.GetInt32("ObjectId");

					typeName = reader.GetString("TypeName");
					isNullable = reader.GetBoolean("is_nullable");
					maxLength = reader.GetInt32OrNull("max_length");
					precision = reader.GetInt32OrNull("precision");
					scale = reader.GetInt32OrNull("scale");
					AdjustTypeDetails(typeName, ref maxLength, ref precision, ref scale, out fullTypeName);
				}
			}

			var objectName = new SqlServerObjectName(actualSchema, actualName);

			var parameters = GetParameters(objectName.ToString(), objectId, con);

			return new ScalarFunctionMetadata<SqlServerObjectName, SqlDbType>(objectName, parameters, typeName, SqlTypeNameToDbType(typeName), isNullable, maxLength, precision, scale, fullTypeName);
		}
	}

	internal StoredProcedureMetadata<SqlServerObjectName, SqlDbType> GetStoredProcedureInternal(SqlServerObjectName procedureName)
	{
		const string StoredProcedureSql =
			@"SELECT
				s.name AS SchemaName,
				sp.name AS Name,
				sp.object_id AS ObjectId
				FROM SYS.procedures sp
				INNER JOIN sys.schemas s ON sp.schema_id = s.schema_id
				WHERE s.name = @Schema AND sp.Name = @Name";

		string actualSchema;
		string actualName;
		int objectId;

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(StoredProcedureSql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", procedureName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name", procedureName.Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.Read())
					{
						var sysStoredProcedure = GetSystemStoredProcedure(procedureName);
						if (sysStoredProcedure != null)
							return sysStoredProcedure;

						var masterStoredProcedure = GetMasterStoredProcedure(procedureName);
						if (masterStoredProcedure != null)
							return masterStoredProcedure;

						throw new MissingObjectException($"Could not find stored procedure {procedureName}");
					}

					actualSchema = reader.GetString("SchemaName");
					actualName = reader.GetString("Name");
					objectId = reader.GetInt32("ObjectId");
				}
			}
			var objectName = new SqlServerObjectName(actualSchema, actualName);
			var parameters = GetParameters(objectName.ToString(), objectId, con);

			return new StoredProcedureMetadata<SqlServerObjectName, SqlDbType>(objectName, parameters);
		}
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is designed to fail silently.")]
	internal StoredProcedureMetadata<SqlServerObjectName, SqlDbType>? GetSystemStoredProcedure(SqlServerObjectName procedureName)
	{
		try
		{
			const string StoredProcedureSql =
				@"SELECT
					s.name AS SchemaName,
					sp.name AS Name,
					sp.object_id AS ObjectId
					FROM SYS.all_objects sp
					INNER JOIN sys.schemas s ON sp.schema_id = s.schema_id
					WHERE s.name = @Schema AND sp.Name = @Name";

			string actualSchema;
			string actualName;
			int objectId;

			using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
			{
				con.Open();
				using (var cmd = new SqlCommand(StoredProcedureSql, con))
				{
					cmd.Parameters.AddWithValue("@Schema", procedureName.Schema ?? "sys");
					cmd.Parameters.AddWithValue("@Name", procedureName.Name);
					using (var reader = cmd.ExecuteReader())
					{
						if (!reader.Read())
							return null;

						actualSchema = reader.GetString("SchemaName");
						actualName = reader.GetString("Name");
						objectId = reader.GetInt32("ObjectId");
					}
				}
				var objectName = new SqlServerObjectName(actualSchema, actualName);
				var parameters = GetParameters(objectName.ToString(), objectId, con, useAllParameters: true);

				return new StoredProcedureMetadata<SqlServerObjectName, SqlDbType>(objectName, parameters);
			}
		}
		catch
		{
			return null; //this will silently fail
		}
	}

	internal TableFunctionMetadata<SqlServerObjectName, SqlDbType> GetTableFunctionInternal(SqlServerObjectName tableFunctionName)
	{
		const string TvfSql =
			@"SELECT
				s.name AS SchemaName,
				o.name AS Name,
				o.object_id AS ObjectId
				FROM sys.objects o
				INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
				WHERE o.type in ('TF', 'IF', 'FT') AND s.name = @Schema AND o.Name = @Name";

		/*
			 * TF = SQL table-valued-function
			 * IF = SQL inline table-valued function
			 * FT = Assembly (CLR) table-valued function
			 */

		string actualSchema;
		string actualName;
		int objectId;

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(TvfSql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", tableFunctionName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name", tableFunctionName.Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.Read())
						throw new MissingObjectException($"Could not find table valued function {tableFunctionName}");
					actualSchema = reader.GetString("SchemaName");
					actualName = reader.GetString("Name");
					objectId = reader.GetInt32("ObjectId");
				}
			}
			var objectName = new SqlServerObjectName(actualSchema, actualName);

			var columns = GetColumns(objectName.ToString(), objectId);
			var parameters = GetParameters(objectName.ToString(), objectId, con);

			return new TableFunctionMetadata<SqlServerObjectName, SqlDbType>(objectName, parameters, columns);
		}
	}

	internal SqlServerTableOrViewMetadata<SqlDbType> GetTableOrViewInternal(SqlServerObjectName tableName)
	{
		const string TableSql =
			@"SELECT
				s.name AS SchemaName,
				t.name AS Name,
				t.object_id AS ObjectId,
				CONVERT(BIT, 1) AS IsTable,
				(SELECT	COUNT(*) FROM sys.triggers t2 WHERE	t2.parent_id = t.object_id) AS Triggers
				FROM SYS.tables t
				INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
				WHERE s.name = @Schema AND t.Name = @Name

				UNION ALL

				SELECT
				s.name AS SchemaName,
				t.name AS Name,
				t.object_id AS ObjectId,
				CONVERT(BIT, 0) AS IsTable,
				(SELECT	COUNT(*) FROM sys.triggers t2 WHERE	t2.parent_id = t.object_id) AS Triggers
				FROM SYS.views t
				INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
				WHERE s.name = @Schema AND t.Name = @Name";

		string actualSchema;
		string actualName;
		int objectId;
		bool isTable;
		bool hasTriggers;

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(TableSql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", tableName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name", tableName.Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.Read())
						throw new MissingObjectException($"Could not find table or view {tableName}");
					actualSchema = reader.GetString("SchemaName");
					actualName = reader.GetString("Name");
					objectId = reader.GetInt32("ObjectId");
					isTable = reader.GetBoolean("IsTable");
					hasTriggers = reader.GetInt32("Triggers") > 0;
				}
			}
		}

		var columns = GetColumns(tableName.ToString(), objectId);

		return new SqlServerTableOrViewMetadata<SqlDbType>(this, new SqlServerObjectName(actualSchema, actualName), isTable, columns, hasTriggers);
	}

	internal UserDefinedTableTypeMetadata<SqlServerObjectName, SqlDbType> GetUserDefinedTableTypeInternal(SqlServerObjectName typeName)
	{
		const string sql =
			@"SELECT	s.name AS SchemaName,
		t.name AS Name,
		tt.type_table_object_id AS ObjectId
FROM	sys.types t
		INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
		LEFT JOIN sys.table_types tt ON tt.user_type_id = t.user_type_id
		LEFT JOIN sys.types t2 ON t.system_type_id = t2.user_type_id
WHERE	s.name = @Schema AND t.name = @Name AND t.is_table_type = 1;";

		string actualSchema;
		string actualName;
		int objectId;

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(sql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", typeName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name", typeName.Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.Read())
						throw new MissingObjectException($"Could not find user defined type {typeName}");

					actualSchema = reader.GetString("SchemaName");
					actualName = reader.GetString("Name");
					objectId = reader.GetInt32("ObjectId");
				}
			}
		}

		var columns = GetColumns(typeName.ToString(), objectId);

		return new UserDefinedTableTypeMetadata<SqlServerObjectName, SqlDbType>(new SqlServerObjectName(actualSchema, actualName), columns);
	}

	/*
		internal UserDefinedTypeMetadata<SqlServerObjectName, SqlDbType> GetUserDefinedTypeInternal(SqlServerObjectName typeName)
		{
			const string sql =
				@"SELECT	s.name AS SchemaName,
	t.name AS Name,
	t2.name AS BaseTypeName,
	t.is_nullable,
	CONVERT(INT, t.max_length) AS max_length,
	CONVERT(INT, t.precision) AS precision,
	CONVERT(INT, t.scale) AS scale
FROM	sys.types t
	INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
	LEFT JOIN sys.types t2 ON t.system_type_id = t2.user_type_id
WHERE	s.name = @Schema AND t.name = @Name AND t.is_table_type = 0;";

			string actualSchema;
			string actualName;
			string baseTypeName;
			bool isTableType;
			bool isNullable;
			int? maxLength;
			int? precision;
			int? scale;
			string fullTypeName;

			using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
			{
				con.Open();
				using (var cmd = new SqlCommand(sql, con))
				{
					cmd.Parameters.AddWithValue("@Schema", typeName.Schema ?? DefaultSchema);
					cmd.Parameters.AddWithValue("@Name", typeName.Name);
					using (var reader = cmd.ExecuteReader())
					{
						if (!reader.Read())
							throw new MissingObjectException($"Could not find user defined type {typeName}");

						actualSchema = reader.GetString("SchemaName");
						actualName = reader.GetString("Name");
						baseTypeName = reader.GetString("BaseTypeName");

						isNullable = reader.GetBoolean("is_nullable");
						maxLength = reader.GetInt32("max_length");
						precision = reader.GetInt32("precision");
						scale = reader.GetInt32("scale");

						AdjustTypeDetails(baseTypeName, ref maxLength, ref precision, ref scale, out fullTypeName);
					}
				}
			}

				var column = new ColumnMetadata<SqlDbType>(null, false, false, false, baseTypeName, SqlTypeNameToDbType(baseTypeName), null, isNullable, maxLength, precision, scale, fullTypeName, ToClrType(baseTypeName, isNullable, maxLength));

				columns = new ColumnMetadataCollection<SqlDbType>(typeName.ToString(), new List<ColumnMetadata<SqlDbType>>() { column });

			return new UserDefinedTableTypeMetadata<SqlServerObjectName, SqlDbType>(new SqlServerObjectName(actualSchema, actualName), isTableType, columns);
		}
		*/

	/// <summary>
	/// Determines the database column type from the column type name.
	/// </summary>
	/// <param name="typeName">Name of the database column type.</param>
	/// <param name="isUnsigned">NOT USED</param>
	/// <returns></returns>
	/// <remarks>This does not honor registered types. This is only used for the database's hard-coded list of native types.</remarks>
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	protected override SqlDbType? SqlTypeNameToDbType(string typeName, bool? isUnsigned = null)
	{
		switch (typeName)
		{
			case "bigint": return SqlDbType.BigInt;
			case "binary": return SqlDbType.Binary;
			case "bit": return SqlDbType.Bit;
			case "char": return SqlDbType.Char;
			case "date": return SqlDbType.Date;
			case "datetime": return SqlDbType.DateTime;
			case "datetime2": return SqlDbType.DateTime2;
			case "datetimeoffset": return SqlDbType.DateTimeOffset;
			case "decimal": return SqlDbType.Decimal;
			case "float": return SqlDbType.Float;
			//case "geography": m_SqlDbType = SqlDbType.;
			//case "geometry": m_SqlDbType = SqlDbType;
			//case "hierarchyid": m_SqlDbType = SqlDbType.;
			case "image": return SqlDbType.Image;
			case "int": return SqlDbType.Int;
			case "money": return SqlDbType.Money;
			case "nchar": return SqlDbType.NChar;
			case "ntext": return SqlDbType.NText;
			case "numeric": return SqlDbType.Decimal;
			case "nvarchar": return SqlDbType.NVarChar;
			case "real": return SqlDbType.Real;
			case "smalldatetime": return SqlDbType.SmallDateTime;
			case "smallint": return SqlDbType.SmallInt;
			case "smallmoney": return SqlDbType.SmallMoney;
			case "sql_variant": return SqlDbType.Variant;
			//case "sysname": m_SqlDbType = SqlDbType;
			case "text": return SqlDbType.Text;
			case "time": return SqlDbType.Time;
			case "timestamp": return SqlDbType.Timestamp;
			case "tinyint": return SqlDbType.TinyInt;
			case "uniqueidentifier": return SqlDbType.UniqueIdentifier;
			case "varbinary": return SqlDbType.VarBinary;
			case "varchar": return SqlDbType.VarChar;
			case "xml": return SqlDbType.Xml;
		}

		return null;
	}

	/// <summary>
	/// Returns the CLR type that matches the indicated database column type.
	/// </summary>
	/// <param name="dbType">Type of the database column.</param>
	/// <param name="isNullable">If nullable, Nullable versions of primitive types are returned.</param>
	/// <param name="maxLength">Optional length. Used to distinguish between a char and string. Defaults to string.</param>
	/// <returns>
	/// A CLR type or NULL if the type is unknown.
	/// </returns>
	/// <remarks>This does not take into consideration registered types.</remarks>
	[SuppressMessage("Microsoft.Maintainability", "CA1502")]
	protected override Type? ToClrType(SqlDbType dbType, bool isNullable, int? maxLength)
	{
		switch (dbType)
		{
			case SqlDbType.BigInt:
				return isNullable ? typeof(long?) : typeof(long);

			case SqlDbType.Binary:
			case SqlDbType.Image:
			case SqlDbType.Timestamp:
			case SqlDbType.VarBinary:
				return typeof(byte[]);

			case SqlDbType.Bit:
				return isNullable ? typeof(bool?) : typeof(bool);

			case SqlDbType.Char:
			case SqlDbType.NChar:
			case SqlDbType.NVarChar:
			case SqlDbType.VarChar:
				return (maxLength == 1) ?
					(isNullable ? typeof(char?) : typeof(char))
					: typeof(string);

			case SqlDbType.DateTime:
			case SqlDbType.Date:
			case SqlDbType.DateTime2:
			case SqlDbType.SmallDateTime:
				return isNullable ? typeof(DateTime?) : typeof(DateTime);

			case SqlDbType.Decimal:
			case SqlDbType.Money:
			case SqlDbType.SmallMoney:
				return isNullable ? typeof(decimal?) : typeof(decimal);

			case SqlDbType.Float:
				return isNullable ? typeof(double?) : typeof(double);

			case SqlDbType.Int:
				return isNullable ? typeof(int?) : typeof(int);

			case SqlDbType.NText:
			case SqlDbType.Text:
			case SqlDbType.Xml:
				return typeof(string);

			case SqlDbType.Real:
				return isNullable ? typeof(float?) : typeof(float);

			case SqlDbType.UniqueIdentifier:
				return isNullable ? typeof(Guid?) : typeof(Guid);

			case SqlDbType.SmallInt:
				return isNullable ? typeof(short?) : typeof(short);

			case SqlDbType.TinyInt:
				return isNullable ? typeof(byte?) : typeof(byte);

			case SqlDbType.Variant:
				return typeof(object);

			case SqlDbType.Time:
				return isNullable ? typeof(TimeSpan?) : typeof(TimeSpan);

			case SqlDbType.DateTimeOffset:
				return isNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);

			case SqlDbType.Udt:
			case SqlDbType.Structured:
				return null;
		}

		return null;
	}

	ColumnMetadataCollection<SqlDbType> GetColumns(string ownerName, int objectId)
	{
		const string ColumnSql =
			@"WITH    PKS
						  AS ( SELECT   c.name ,
										1 AS is_primary_key
							   FROM     sys.indexes i
										INNER JOIN sys.index_columns ic ON i.index_id = ic.index_id
																		   AND ic.object_id = @ObjectId
										INNER JOIN sys.columns c ON ic.column_id = c.column_id
																	AND c.object_id = @ObjectId
							   WHERE    i.is_primary_key = 1
										AND ic.is_included_column = 0
										AND i.object_id = @ObjectId
							 )
					SELECT  c.name AS ColumnName ,
							c.is_computed ,
							c.is_identity ,
							c.column_id ,
							Convert(bit, ISNULL(PKS.is_primary_key, 0)) AS is_primary_key,
							COALESCE(t.name, t2.name) AS TypeName,
							c.is_nullable,
							CONVERT(INT, COALESCE(c.max_length, t.max_length, t2.max_length)) AS max_length,
							CONVERT(INT, COALESCE(c.precision, t.precision, t2.precision)) AS precision,
							CONVERT(INT, COALESCE(c.scale, t.scale, t2.scale)) AS scale
					FROM    sys.columns c
							LEFT JOIN PKS ON c.name = PKS.name
							LEFT JOIN sys.types t on c.system_type_id = t.user_type_id
							LEFT JOIN sys.types t2 ON c.user_type_id = t2.user_type_id
							WHERE   object_id = @ObjectId;";

		var columns = new List<ColumnMetadata<SqlDbType>>();
		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new SqlCommand(ColumnSql, con))
			{
				cmd.Parameters.AddWithValue("@ObjectId", objectId);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var name = reader.GetString("ColumnName");
						var computed = reader.GetBoolean("is_computed");
						var primary = reader.GetBoolean("is_primary_key");
						var isIdentity = reader.GetBoolean("is_identity");
						var typeName = reader.GetString("TypeName");
						var isNullable = reader.GetBoolean("is_nullable");
						int? maxLength = reader.GetInt32OrNull("max_length");
						int? precision = reader.GetInt32OrNull("precision");
						int? scale = reader.GetInt32OrNull("scale");
						string fullTypeName;
						AdjustTypeDetails(typeName, ref maxLength, ref precision, ref scale, out fullTypeName);

						columns.Add(new ColumnMetadata<SqlDbType>(name, computed, primary, isIdentity, typeName, SqlTypeNameToDbType(typeName), QuoteColumnName(name), isNullable, maxLength, precision, scale, fullTypeName, ToClrType(typeName, isNullable, maxLength)));
					}
				}
			}
		}
		return new ColumnMetadataCollection<SqlDbType>(ownerName, columns);
	}

	List<SqlServerIndexColumnMetadata> GetColumnsForIndex(SqlServerObjectName tableName)
	{
		const string columnSql = @"SELECT c.name,
	   ic.is_descending_key,
	   ic.is_included_column,
	   ic.index_id
FROM sys.index_columns ic
	INNER JOIN sys.objects o
		ON ic.object_id = o.object_id
	INNER JOIN sys.schemas s
		ON o.schema_id = s.schema_id
	INNER JOIN sys.columns c
		ON ic.object_id = c.object_id
		   AND ic.column_id = c.column_id
WHERE o.name = @Name
	  AND s.name = @Schema
ORDER BY ic.key_ordinal;";

		var tableColumns = GetTableOrView(tableName).Columns;

		using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();

			using (var cmd = new SqlCommand(columnSql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", tableName.Schema);
				cmd.Parameters.AddWithValue("@Name", tableName.Name);

				using (var reader = cmd.ExecuteReader())
				{
					var results = new List<SqlServerIndexColumnMetadata>();

					while (reader.Read())
					{
						var is_included_column = reader.GetBoolean("is_included_column");
						var is_descending_key = reader.GetBooleanOrNull("is_descending_key");
						var name = reader.GetString("Name");
						var index_id = reader.GetInt32("index_id");
						var column = tableColumns[name];

						results.Add(new SqlServerIndexColumnMetadata(column, is_descending_key, is_included_column, index_id));
					}
					return results;
				}
			}
		}
	}

	//ParameterMetadataCollection<SqlDbType> GetParameters(string procedureName, int objectId)
	//{
	//    using (var con = new SqlConnection(m_ConnectionBuilder.ConnectionString))
	//    {
	//        con.Open();
	//        return GetParameters(procedureName, objectId, con);
	//    }
	//}

	ParameterMetadataCollection<SqlDbType> GetParameters(string procedureName, int objectId, SqlConnection con, bool useAllParameters = false)
	{
		try
		{
			const string ParameterSqlA =
				@"SELECT  p.name AS ParameterName ,
			COALESCE(t.name, t2.name) AS TypeName,
			COALESCE(t.is_nullable, t2.is_nullable)  as is_nullable,
			CONVERT(INT, t.max_length) AS max_length,
			CONVERT(INT, t.precision) AS precision,
			CONVERT(INT, t.scale) AS scale,
			p.is_output
			FROM    sys.parameters p
					LEFT JOIN sys.types t ON p.system_type_id = t.user_type_id
					LEFT JOIN sys.types t2 ON p.user_type_id = t2.user_type_id
			WHERE   p.object_id = @ObjectId AND p.parameter_id <> 0
			ORDER BY p.parameter_id;";

			const string ParameterSqlB =
				@"SELECT  p.name AS ParameterName ,
			COALESCE(t.name, t2.name) AS TypeName,
			COALESCE(t.is_nullable, t2.is_nullable)  as is_nullable,
			CONVERT(INT, t.max_length) AS max_length,
			CONVERT(INT, t.precision) AS precision,
			CONVERT(INT, t.scale) AS scale,
			p.is_output
			FROM    sys.all_parameters p
					LEFT JOIN sys.types t ON p.system_type_id = t.user_type_id
					LEFT JOIN sys.types t2 ON p.user_type_id = t2.user_type_id
			WHERE   p.object_id = @ObjectId AND p.parameter_id <> 0
			ORDER BY p.parameter_id;";

			//we exclude parameter_id 0 because it is the return type of scalar functions.
			//we need to use all_parameters for system stored procedures

			var ParameterSql = !useAllParameters ? ParameterSqlA : ParameterSqlB;

			var parameters = new List<ParameterMetadata<SqlDbType>>();

			using (var cmd = new SqlCommand(ParameterSql, con))
			{
				cmd.Parameters.AddWithValue("@ObjectId", objectId);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var name = reader.GetString("ParameterName");
						var typeName = reader.GetString("TypeName");
						const bool isNullable = true;
						var maxLength = reader.GetInt32OrNull("max_length");
						var precision = reader.GetInt32OrNull("precision");
						var scale = reader.GetInt32OrNull("scale");
						var isOutput = reader.GetBoolean("is_output");
						string fullTypeName;
						AdjustTypeDetails(typeName, ref maxLength, ref precision, ref scale, out fullTypeName);

						var direction = isOutput ? ParameterDirection.Output : ParameterDirection.Input;

						parameters.Add(new ParameterMetadata<SqlDbType>(name, name, typeName, SqlTypeNameToDbType(typeName), isNullable, maxLength, precision, scale, fullTypeName, direction));
					}
				}
			}

			return new ParameterMetadataCollection<SqlDbType>(procedureName, parameters);
		}
		catch (Exception ex)
		{
			throw new MetadataException($"Error getting parameters for {procedureName}", ex);
		}
	}

	class SqlServerIndexColumnMetadata : IndexColumnMetadata<SqlDbType>
	{
		/// <summary>
		/// Initializes a new instance of the IndexColumnMetadata class.
		/// </summary>
		/// <param name="column">The underlying column details.</param>
		/// <param name="isDescending">Indicates the column is indexed in descending order.</param>
		/// <param name="isIncluded">Indicates the column is an unindexed, included column.</param>
		/// <param name="indexId"></param>
		internal SqlServerIndexColumnMetadata(ColumnMetadata<SqlDbType> column, bool? isDescending, bool isIncluded, int indexId) : base(column, isDescending, isIncluded)
		{
			IndexId = indexId;
		}

		internal int IndexId { get; }
	}
}
