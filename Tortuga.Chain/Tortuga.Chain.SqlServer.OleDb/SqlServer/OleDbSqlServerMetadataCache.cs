using System.Collections.Immutable;
using System.Data.OleDb;
using System.Diagnostics.CodeAnalysis;
using Tortuga.Chain.Aggregates;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.SqlServer;

/// <summary>
/// Class OleDbSqlServerMetadataCache.
/// </summary>
/// <seealso cref="DatabaseMetadataCache{ SqlServerObjectName, OleDbType}" />
public sealed partial class OleDbSqlServerMetadataCache
{
	/// <summary>
	/// Initializes a new instance of the <see cref="OleDbSqlServerMetadataCache"/> class.
	/// </summary>
	/// <param name="connectionBuilder">The connection builder.</param>
	public OleDbSqlServerMetadataCache(OleDbConnectionStringBuilder connectionBuilder)
	{
		m_ConnectionBuilder = connectionBuilder;
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
				using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
				{
					con.Open();
					using (var cmd = new OleDbCommand("SELECT DB_NAME () AS DatabaseName", con))
					{
						m_DatabaseName = (string)cmd.ExecuteScalar()!;
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
				using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
				{
					con.Open();
					using (var cmd = new OleDbCommand("SELECT SCHEMA_NAME () AS DefaultSchema", con))
					{
						m_DefaultSchema = (string)cmd.ExecuteScalar()!;
					}
				}
			}
			return m_DefaultSchema;
		}
	}

	/// <summary>
	/// Gets a list of known, unsupported SQL type names that cannot be mapped to a OleDbType.
	/// </summary>
	/// <value>Case-insensitive list of database-specific type names</value>
	/// <remarks>This list is based on driver limitations.</remarks>
	public override ImmutableHashSet<string> UnsupportedSqlTypeNames { get; } = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, new[] { "datetimeoffset", "geography", "geometry", "hierarchyid", "image", "sql_variant", "sysname", "xml" });

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
	/// Gets the detailed metadata for a table or view.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <returns>SqlServerTableOrViewMetadata&lt;TDbType&gt;.</returns>
	public new SqlServerTableOrViewMetadata<OleDbType> GetTableOrView(SqlServerObjectName tableName)
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

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(TvfSql, con))
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

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(StoredProcedureSql, con))
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

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(TvfSql, con))
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

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(tableList, con))
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
		const string tableList = @"SELECT s.name AS SchemaName, t.name AS Name FROM sys.types t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE	t.is_user_defined = 1;";

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(tableList, con))
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

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(tableList, con))
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

		return "[" + columnName + "]";
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
	public override string ValueToSqlValue(object value, OleDbType? dbType)
	{
		switch (value)
		{
			case string s:
				{
					switch (dbType)
					{
						case OleDbType.Char:
						case OleDbType.LongVarChar:
						case OleDbType.VarChar:
							return "'" + s.Replace("'", "''", StringComparison.OrdinalIgnoreCase) + "'";

						case OleDbType.WChar:
						case OleDbType.BSTR:
						case OleDbType.VarWChar:
						case OleDbType.LongVarWChar:
							return "N'" + s.Replace("'", "''", StringComparison.OrdinalIgnoreCase) + "'";

						default: //Assume Unicode
							return "N'" + s.Replace("'", "''", StringComparison.OrdinalIgnoreCase) + "'";
					}
				}

			default:
				return base.ValueToSqlValue(value, dbType);
		}
	}

	internal ScalarFunctionMetadata<SqlServerObjectName, OleDbType> GetScalarFunctionInternal(SqlServerObjectName tableFunctionName)
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
		AND s.name = ?
		AND o.name = ?;";

		string actualSchema;
		string actualName;
		int objectId;

		string fullTypeName;
		string typeName;
		bool isNullable;
		int? maxLength;
		int? precision;
		int? scale;

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(sql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", tableFunctionName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name", tableFunctionName.Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.Read())
						throw new MissingObjectException($"Could not find scalar function {tableFunctionName}");
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
		}
		var objectName = new SqlServerObjectName(actualSchema, actualName);

		var parameters = GetParameters(objectName.ToString(), objectId);

		return new ScalarFunctionMetadata<SqlServerObjectName, OleDbType>(objectName, parameters, typeName, SqlTypeNameToDbType(typeName), isNullable, maxLength, precision, scale, fullTypeName);
	}

	internal StoredProcedureMetadata<SqlServerObjectName, OleDbType> GetStoredProcedureInternal(SqlServerObjectName procedureName)
	{
		const string StoredProcedureSql =
			@"SELECT
				s.name AS SchemaName,
				sp.name AS Name,
				sp.object_id AS ObjectId
				FROM SYS.procedures sp
				INNER JOIN sys.schemas s ON sp.schema_id = s.schema_id
				WHERE s.name = ? AND sp.Name = ?";

		string actualSchema;
		string actualName;
		int objectId;

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(StoredProcedureSql, con))
			{
				cmd.Parameters.AddWithValue("@Schema", procedureName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name", procedureName.Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (!reader.Read())
						throw new MissingObjectException($"Could not find stored procedure {procedureName}");

					actualSchema = reader.GetString("SchemaName");
					actualName = reader.GetString("Name");
					objectId = reader.GetInt32("ObjectId");
				}
			}
		}
		var objectName = new SqlServerObjectName(actualSchema, actualName);
		var parameters = GetParameters(objectName.ToString(), objectId);

		return new StoredProcedureMetadata<SqlServerObjectName, OleDbType>(objectName, parameters);
	}

	internal TableFunctionMetadata<SqlServerObjectName, OleDbType> GetTableFunctionInternal(SqlServerObjectName tableFunctionName)
	{
		const string TvfSql =
			@"SELECT
				s.name AS SchemaName,
				o.name AS Name,
				o.object_id AS ObjectId
				FROM sys.objects o
				INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
				WHERE o.type in ('TF', 'IF', 'FT') AND s.name = ? AND o.Name = ?";

		/*
			 * TF = SQL table-valued-function
			 * IF = SQL inline table-valued function
			 * FT = Assembly (CLR) table-valued function
			 */

		string actualSchema;
		string actualName;
		int objectId;

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(TvfSql, con))
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
		}
		var objectName = new SqlServerObjectName(actualSchema, actualName);

		var columns = GetColumns(tableFunctionName.ToString(), objectId);
		var parameters = GetParameters(objectName.ToString(), objectId);

		return new TableFunctionMetadata<SqlServerObjectName, OleDbType>(objectName, parameters, columns);
	}

	internal SqlServerTableOrViewMetadata<OleDbType> GetTableOrViewInternal(SqlServerObjectName tableName)
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
				WHERE s.name = ? AND t.Name = ?

				UNION ALL

				SELECT
				s.name AS SchemaName,
				t.name AS Name,
				t.object_id AS ObjectId,
				CONVERT(BIT, 0) AS IsTable,
				(SELECT	COUNT(*) FROM sys.triggers t2 WHERE	t2.parent_id = t.object_id) AS Triggers
				FROM SYS.views t
				INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
				WHERE s.name = ? AND t.Name = ?";

		string actualSchema;
		string actualName;
		int objectId;
		bool isTable;
		bool hasTriggers;

		using (var con = new OleDbConnection(m_ConnectionBuilder!.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(TableSql, con))
			{
				cmd.Parameters.AddWithValue("@Schema1", tableName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name1", tableName.Name);
				cmd.Parameters.AddWithValue("@Schema2", tableName.Schema ?? DefaultSchema);
				cmd.Parameters.AddWithValue("@Name2", tableName.Name);
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

		return new SqlServerTableOrViewMetadata<OleDbType>(this, new SqlServerObjectName(actualSchema, actualName), isTable, columns, hasTriggers);
	}

	internal UserDefinedTableTypeMetadata<SqlServerObjectName, OleDbType>
		GetUserDefinedTableTypeInternal(SqlServerObjectName typeName)
	{
		const string sql =
			@"SELECT	s.name AS SchemaName,
		t.name AS Name,
		tt.type_table_object_id AS ObjectId
FROM	sys.types t
		INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
		LEFT JOIN sys.table_types tt ON tt.user_type_id = t.user_type_id
		LEFT JOIN sys.types t2 ON t.system_type_id = t2.user_type_id
WHERE	s.name = ? AND t.name = ? AND t.is_table_type = 1;";

		string actualSchema;
		string actualName;

		int objectId;

		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(sql, con))
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

		return new UserDefinedTableTypeMetadata<SqlServerObjectName, OleDbType>(new SqlServerObjectName(actualSchema, actualName), columns);
	}

	/// <summary>
	/// Determines the database column type from the column type name.
	/// </summary>
	/// <param name="typeName">Name of the database column type.</param>
	/// <param name="isUnsigned">NOT USED</param>
	/// <returns></returns>
	/// <remarks>This does not honor registered types. This is only used for the database's hard-coded list of native types.</remarks>
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	protected override OleDbType? SqlTypeNameToDbType(string typeName, bool? isUnsigned = null)
	{
		switch (typeName)
		{
			case "bigint": return OleDbType.BigInt;
			case "binary": return OleDbType.Binary;
			case "bit": return OleDbType.Boolean;
			case "char": return OleDbType.Char;
			case "date": return OleDbType.DBDate;
			case "datetime": return OleDbType.DBTimeStamp;
			case "datetime2": return OleDbType.DBTimeStamp;
			//case "datetimeoffset": return OleDbType;
			case "decimal": return OleDbType.Decimal;
			case "float": return OleDbType.Single;
			//case "geography": m_SqlDbType = OleDbType.;
			//case "geometry": m_SqlDbType = OleDbType;
			//case "hierarchyid": m_SqlDbType = OleDbType.;
			//case "image": return OleDbType.Image;
			case "int": return OleDbType.Integer;
			case "money": return OleDbType.Currency;
			case "nchar": return OleDbType.WChar;
			case "ntext": return OleDbType.LongVarWChar;
			case "numeric": return OleDbType.Numeric;
			case "nvarchar": return OleDbType.VarWChar;
			case "real": return OleDbType.Single;
			case "smalldatetime": return OleDbType.DBTimeStamp;
			case "smallint": return OleDbType.SmallInt;
			case "smallmoney": return OleDbType.Currency;
			//case "sql_variant": m_SqlDbType = OleDbType;
			//case "sysname": m_SqlDbType = OleDbType;
			case "text": return OleDbType.LongVarWChar;
			case "time": return OleDbType.DBTime;
			case "timestamp": return OleDbType.DBTimeStamp;
			case "tinyint": return OleDbType.TinyInt;
			case "uniqueidentifier": return OleDbType.Guid;
			case "varbinary": return OleDbType.VarBinary;
			case "varchar": return OleDbType.VarChar;
				//case "xml": return OleDbType;
		}

		return null;
	}

	ColumnMetadataCollection<OleDbType> GetColumns(string ownerName, int objectId)
	{
		const string ColumnSql =
			@"WITH    PKS
						  AS ( SELECT   c.name ,
										1 AS is_primary_key
							   FROM     sys.indexes i
										INNER JOIN sys.index_columns ic ON i.index_id = ic.index_id
																		   AND ic.object_id = ?
										INNER JOIN sys.columns c ON ic.column_id = c.column_id
																	AND c.object_id = ?
							   WHERE    i.is_primary_key = 1
										AND ic.is_included_column = 0
										AND i.object_id = ?
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
							WHERE   object_id = ?;";

		var columns = new List<ColumnMetadata<OleDbType>>();
		using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
		{
			con.Open();
			using (var cmd = new OleDbCommand(ColumnSql, con))
			{
				cmd.Parameters.AddWithValue("@ObjectId1", objectId);
				cmd.Parameters.AddWithValue("@ObjectId2", objectId);
				cmd.Parameters.AddWithValue("@ObjectId3", objectId);
				cmd.Parameters.AddWithValue("@ObjectId4", objectId);
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

						columns.Add(new ColumnMetadata<OleDbType>(name, computed, primary, isIdentity, typeName, SqlTypeNameToDbType(typeName), QuoteColumnName(name), isNullable, maxLength, precision, scale, fullTypeName, ToClrType(typeName, isNullable, maxLength)));
					}
				}
			}
		}
		return new ColumnMetadataCollection<OleDbType>(ownerName, columns);
	}

	ParameterMetadataCollection<OleDbType> GetParameters(string procedureName, int objectId)
	{
		try
		{
			const string ParameterSql =
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
			WHERE   p.object_id = ? AND p.parameter_id <> 0
			ORDER BY p.parameter_id;";

			//we exclude parameter_id 0 because it is the return type of scalar functions.

			var parameters = new List<ParameterMetadata<OleDbType>>();

			using (var con = new OleDbConnection(m_ConnectionBuilder.ConnectionString))
			{
				con.Open();

				using (var cmd = new OleDbCommand(ParameterSql, con))
				{
					cmd.Parameters.AddWithValue("@ObjectId", objectId);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var name = reader.GetString("ParameterName");
							var typeName = reader.GetString("TypeName");
							bool isNullable = true;
							int? maxLength = reader.GetInt32OrNull("max_length");
							int? precision = reader.GetInt32OrNull("precision");
							int? scale = reader.GetInt32OrNull("scale");
							var isOutput = reader.GetBoolean("is_output");

							string fullTypeName;
							AdjustTypeDetails(typeName, ref maxLength, ref precision, ref scale, out fullTypeName);

							var direction = isOutput ? ParameterDirection.Output : ParameterDirection.Input;

							parameters.Add(new ParameterMetadata<OleDbType>(name, name, typeName, SqlTypeNameToDbType(typeName), isNullable, maxLength, precision, scale, fullTypeName, direction));
						}
					}
				}
			}
			return new ParameterMetadataCollection<OleDbType>(procedureName, parameters);
		}
		catch (Exception ex)
		{
			throw new MetadataException($"Error getting parameters for {procedureName}", ex);
		}
	}
}
