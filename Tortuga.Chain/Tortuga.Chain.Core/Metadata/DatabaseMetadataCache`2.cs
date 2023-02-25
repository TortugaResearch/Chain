using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Tortuga.Anchor;
using Tortuga.Anchor.Metadata;
using Tortuga.Chain.Aggregates;

namespace Tortuga.Chain.Metadata;

/// <summary>
/// An abstract database metadata cache
/// </summary>
/// <typeparam name="TObjectName">The type used to represent database object names.</typeparam>
/// <typeparam name="TDbType">The variant of DbType used by this data source.</typeparam>
public abstract class DatabaseMetadataCache<TObjectName, TDbType> : IDatabaseMetadataCache
	where TObjectName : struct
	where TDbType : struct
{
	/// <summary>
	/// This dictionary is used to register customer database types. It is used by the ToClrType method and possibly parameter generation.
	/// </summary>
	/// <remarks>This is populated by the RegisterType method.</remarks>
	readonly ConcurrentDictionary<string, TypeRegistration<TDbType>> m_RegisteredTypes = new ConcurrentDictionary<string, TypeRegistration<TDbType>>(StringComparer.OrdinalIgnoreCase);

	private readonly ConcurrentDictionary<(Type, OperationType), TableOrViewMetadata<TObjectName, TDbType>> m_TypeTableMap = new ConcurrentDictionary<(Type, OperationType), TableOrViewMetadata<TObjectName, TDbType>>();

	/// <summary>
	/// Gets the converter dictionary used by materializers.
	/// </summary>
	/// <value>The converter dictionary.</value>
	public MaterializerTypeConverter Converter { get; } = new();

	/// <summary>
	/// Gets the maximum number of parameters in a single SQL batch.
	/// </summary>
	/// <value>The maximum number of parameters.</value>
	public virtual int? MaxParameters => null;

	/// <summary>
	/// Get the maximum number of rows in a single SQL statement's Values clause.
	/// </summary>
	public virtual int? MaxRowsPerValuesClause => null;

	/// <summary>
	/// Gets the server version number.
	/// </summary>
	public virtual Version? ServerVersion => null;

	/// <summary>
	/// Gets the server version name.
	/// </summary>
	public virtual string? ServerVersionName => null;

	/// <summary>
	/// Gets a list of known, unsupported SQL type names.
	/// </summary>
	/// <value>Case-insensitive list of database-specific type names</value>
	/// <remarks>This list is based on driver limitations.</remarks>
	public virtual ImmutableHashSet<string> UnsupportedSqlTypeNames => ImmutableHashSet<string>.Empty;

	/// <summary>
	/// Gets an aggregate function.
	/// </summary>
	/// <param name="aggregateType">Type of the aggregate.</param>
	/// <param name="columnName">Name of the column to insert into the function.</param>
	/// <returns>A string suitable for use in an aggregate.</returns>
	public virtual string GetAggregateFunction(AggregateType aggregateType, string columnName)
	{
		switch (aggregateType)
		{
			case AggregateType.Min:
				return $"MIN({QuoteColumnName(columnName!)})";

			case AggregateType.Max:
				return $"MAX({QuoteColumnName(columnName!)})";

			case AggregateType.Average:
				return $"AVG({QuoteColumnName(columnName!)})";

			case AggregateType.Count:
				return $"COUNT({QuoteColumnName(columnName!)})";

			case AggregateType.CountDistinct:
				return $"COUNT(DISTINCT {QuoteColumnName(columnName!)})";

			case AggregateType.Sum:
				return $"SUM({QuoteColumnName(columnName!)})";

			case AggregateType.SumDistinct:
				return $"SUM(DISTINCT {QuoteColumnName(columnName!)})";

			default:
				throw new ArgumentOutOfRangeException(nameof(AggregateType));
		}
	}

	/// <summary>
	/// Gets the foreign keys for a table.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <returns></returns>
	/// <exception cref="NotSupportedException">Foreign keys are not supported by this data source</exception>
	/// <remarks>
	/// This should be cached on a TableOrViewMetadata object.
	/// </remarks>
	public virtual ForeignKeyConstraintCollection<TObjectName, TDbType> GetForeignKeysForTable(TObjectName tableName)
	{
		throw new NotSupportedException("Foreign Keys are not supported by this data source");
	}

	/// <summary>
	/// Gets the indexes for a table.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <returns></returns>
	/// <exception cref="NotSupportedException">Indexes are not supported by this data source</exception>
	/// <remarks>
	/// This should be cached on a TableOrViewMetadata object.
	/// </remarks>
	public virtual IndexMetadataCollection<TObjectName, TDbType> GetIndexesForTable(TObjectName tableName)
	{
		throw new NotSupportedException("Indexes are not supported by this data source");
	}

	///// <summary>
	///// Gets the parameters from a SQL Builder.
	///// </summary>
	///// <param name="sqlBuilder">The SQL builder.</param>
	///// <returns></returns>
	//public List<TParameter> GetParameters(SqlBuilder<TDbType> sqlBuilder)
	//{
	//	return sqlBuilder.GetParameters(ParameterBuilderCallback);
	//}

	/// <summary>
	/// Gets the metadata for a scalar function.
	/// </summary>
	/// <param name="scalarFunctionName">Name of the scalar function.</param>
	/// <returns>Null if the object could not be found.</returns>
	public virtual ScalarFunctionMetadata<TObjectName, TDbType> GetScalarFunction(TObjectName scalarFunctionName)
	{
		throw new NotSupportedException("Table value functions are not supported by this data source");
	}

	/// <summary>
	/// Gets the scalar functions that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// Call Preload before invoking this method to ensure that all scalar functions were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.
	/// </remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public virtual IReadOnlyCollection<ScalarFunctionMetadata<TObjectName, TDbType>> GetScalarFunctions()
	{
		throw new NotSupportedException("Table value functions are not supported by this data source");
	}

	/// <summary>
	/// Gets the stored procedure's metadata.
	/// </summary>
	/// <param name="procedureName">Name of the procedure.</param>
	/// <returns></returns>
	public virtual StoredProcedureMetadata<TObjectName, TDbType> GetStoredProcedure(TObjectName procedureName)
	{
		throw new NotSupportedException("Stored procedures are not supported by this data source");
	}

	StoredProcedureMetadata IDatabaseMetadataCache.GetStoredProcedure(string procedureName)
	{
		return GetStoredProcedure(ParseObjectName(procedureName));
	}

	/// <summary>
	/// Gets the stored procedures that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Call Preload before invoking this method to ensure that all stored procedures were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.</remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public virtual IReadOnlyCollection<StoredProcedureMetadata<TObjectName, TDbType>> GetStoredProcedures()
	{
		throw new NotSupportedException("Stored procedures are not supported by this data source");
	}

	IReadOnlyCollection<StoredProcedureMetadata> IDatabaseMetadataCache.GetStoredProcedures()
	{
		return GetStoredProcedures();
	}

	/// <summary>
	/// Gets the metadata for a table function.
	/// </summary>
	/// <param name="tableFunctionName">Name of the table function.</param>
	public virtual TableFunctionMetadata<TObjectName, TDbType> GetTableFunction(TObjectName tableFunctionName)
	{
		throw new NotSupportedException("Table value functions are not supported by this data source");
	}

	TableFunctionMetadata IDatabaseMetadataCache.GetTableFunction(string tableFunctionName)
	{
		return GetTableFunction(ParseObjectName(tableFunctionName));
	}

	/// <summary>
	/// Gets the table-valued functions that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Call Preload before invoking this method to ensure that all table-valued functions were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.</remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public virtual IReadOnlyCollection<TableFunctionMetadata<TObjectName, TDbType>> GetTableFunctions()
	{
		throw new NotSupportedException("Table value functions are not supported by this data source");
	}

	IReadOnlyCollection<TableFunctionMetadata> IDatabaseMetadataCache.GetTableFunctions() => GetTableFunctions();

	/// <summary>
	/// Gets the metadata for a table.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <returns></returns>
	public abstract TableOrViewMetadata<TObjectName, TDbType> GetTableOrView(TObjectName tableName);

	TableOrViewMetadata IDatabaseMetadataCache.GetTableOrView(string tableName) => GetTableOrView(ParseObjectName(tableName));

	/// <summary>
	/// Returns the table or view derived from the class's name and/or Table attribute.
	/// </summary>
	/// <typeparam name="TObject">The type of the object.</typeparam>
	/// <param name="operation">The operation.</param>
	/// <returns>DatabaseObject.</returns>
	public TableOrViewMetadata<TObjectName, TDbType> GetTableOrViewFromClass<TObject>(OperationType operation = OperationType.All)
	{
		var type = typeof(TObject);
		return GetTableOrViewFromClass(type, operation);
	}

	/// <summary>
	/// Returns the table or view derived from the class's name and/or Table attribute.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="operation">The operation.</param>
	/// <returns>DatabaseObject.</returns>
	public TableOrViewMetadata<TObjectName, TDbType> GetTableOrViewFromClass(Type type, OperationType operation = OperationType.All)
	{
		var cacheKey = (type, operation);

		if (m_TypeTableMap.TryGetValue(cacheKey, out var cachedResult))
			return cachedResult;

		//This section uses the TableAttribute or TableAndViewAttribute from the class.
		var typeInfo = MetadataCache.GetMetadata(type);
		var objectName = typeInfo.MappedTableName;
		var schemaName = typeInfo.MappedSchemaName;

		//On reads we can use the view instead.
		if (operation == OperationType.Select)
		{
			objectName = typeInfo.MappedViewName ?? objectName;
			schemaName = typeInfo.MappedViewSchemaName ?? schemaName;
		}

		if (!objectName.IsNullOrEmpty())
		{
			var name = schemaName.IsNullOrEmpty() ? ParseObjectName(objectName) : ParseObjectName(schemaName, objectName);

			if (TryGetTableOrView(name, out var tableResult))
			{
				m_TypeTableMap[cacheKey] = tableResult;
				return tableResult;
			}

			throw new MissingObjectException($"Cannot find a table or view with the name {name}");
		}

		//This section infers the schema from namespace
		{
			var schema = type.Namespace;
			if (schema != null && schema.Contains(".", StringComparison.Ordinal))
				schema = schema.Substring(schema.LastIndexOf(".", StringComparison.Ordinal) + 1);

			var nameA = ParseObjectName(schema, type.Name);
			var nameB = ParseObjectName(null, type.Name);

			if (TryGetTableOrView(nameA, out var tableResult))
			{
				m_TypeTableMap[cacheKey] = tableResult;
				return tableResult;
			}

			//that didn't work, so try the default schema
			if (TryGetTableOrView(nameB, out var tableResult2))
			{
				m_TypeTableMap[cacheKey] = tableResult2;
				return tableResult2;
			}

			throw new MissingObjectException($"Cannot find a table or view with the name '{nameA}' or '{nameB}'");
		}
	}

	TableOrViewMetadata IDatabaseMetadataCache.GetTableOrViewFromClass<TObject>(OperationType operation)
				=> GetTableOrViewFromClass<TObject>(operation);

	TableOrViewMetadata IDatabaseMetadataCache.GetTableOrViewFromClass(Type type, OperationType operation)
				=> GetTableOrViewFromClass(type, operation);

	/// <summary>DatabaseObject
	/// Gets the tables and views that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Call Preload before invoking this method to ensure that all tables and views were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.</remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public abstract IReadOnlyCollection<TableOrViewMetadata<TObjectName, TDbType>> GetTablesAndViews();

	IReadOnlyCollection<TableOrViewMetadata> IDatabaseMetadataCache.GetTablesAndViews() => GetTablesAndViews();

	/// <summary>
	/// Gets the metadata for a user defined type.
	/// </summary>
	/// <param name="typeName">Name of the type.</param>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public virtual UserDefinedTableTypeMetadata<TObjectName, TDbType> GetUserDefinedTableType(TObjectName typeName)
	{
		throw new NotSupportedException("User defined types are not supported by this data source");
	}

	UserDefinedTableTypeMetadata IDatabaseMetadataCache.GetUserDefinedTableType(string typeName) => GetUserDefinedTableType(ParseObjectName(typeName));

	IReadOnlyCollection<UserDefinedTableTypeMetadata> IDatabaseMetadataCache.GetUserDefinedTableTypes() => GetUserDefinedTableTypes();

	/// <summary>
	/// Gets the table-valued functions that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Call Preload before invoking this method to ensure that all table-valued functions were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.</remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public virtual IReadOnlyCollection<UserDefinedTableTypeMetadata<TObjectName, TDbType>> GetUserDefinedTableTypes()
	{
		throw new NotSupportedException("Table value functions are not supported by this data source");
	}

	/// <summary>
	/// Parse a string and return the database specific representation of the database object's name.
	/// </summary>
	/// <param name="name">Name of the object.</param>
	public TObjectName ParseObjectName(string name) => ParseObjectName(null, name);

	/// <summary>
	/// Preloads all of the metadata for this data source.
	/// </summary>
	public abstract void Preload();

	/// <summary>
	/// Quotes the name of the column.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <returns>System.String.</returns>
	/// <remarks>This assumes the column name wasn't already quoted.</remarks>
	public abstract string QuoteColumnName(string columnName);

	/// <summary>
	/// Parse a string and return the database specific representation of the database object's name as a quoted string.
	/// </summary>
	/// <param name="name">Name of the object.</param>
	public abstract string QuoteObjectName(string name);

	/// <summary>
	/// Registers a database type and its CLR equivalent.
	/// </summary>
	/// <param name="databaseTypeName">Name of the database type.</param>
	/// <param name="databaseType">Type of the database.</param>
	/// <param name="clrType">Type of the color.</param>
	public void RegisterType(string databaseTypeName, TDbType databaseType, Type clrType)
	{
		m_RegisteredTypes[databaseTypeName] = new TypeRegistration<TDbType>(databaseTypeName, databaseType, clrType);
	}

	/// <summary>
	/// Resets the metadata cache, clearing out all cached metadata.
	/// </summary>
	public virtual void Reset()
	{
		m_TypeTableMap.Clear();
	}

	/// <summary>
	/// Returns the CLR type that matches the indicated database column type.
	/// </summary>
	/// <param name="typeName">Name of the database column type.</param>
	/// <param name="isNullable">If nullable, Nullable versions of primitive types are returned.</param>
	/// <param name="maxLength">Optional length. Used to distinguish between a char and string.</param>
	/// <param name="isUnsigned">Indicates whether or not the column is unsigned. Only applicable to some databases.</param>
	/// <returns>
	/// A CLR type or NULL if the type is unknown.
	/// </returns>
	/// <remarks>Use RegisterType to add a missing mapping or override an existing one.</remarks>
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public Type? ToClrType(string typeName, bool isNullable, int? maxLength, bool? isUnsigned = null)
	{
		if (TryGetRegisteredType(typeName, out var registeredType))
			return registeredType.ClrType;

		var dbType = SqlTypeNameToDbType(typeName, isUnsigned);

		if (dbType.HasValue)
			return ToClrType(dbType.Value, isNullable, maxLength);

		return null;
	}

	/// <summary>
	/// Tries to get the metadata for a scalar function.
	/// </summary>
	/// <param name="tableFunctionName">Name of the scalar function.</param>
	/// <param name="tableFunction">The scalar function.</param>
	/// <returns></returns>
	public bool TryGetScalarFunction(string tableFunctionName, [NotNullWhen(true)] out ScalarFunctionMetadata? tableFunction) =>
		TryGetScalarFunction(ParseObjectName(tableFunctionName), out tableFunction);

	/// <summary>
	/// Tries to get the metadata for a scalar function.
	/// </summary>
	/// <param name="scalarFunctionName">Name of the scalar function.</param>
	/// <param name="scalarFunction">The scalar function.</param>
	/// <returns></returns>
	public bool TryGetScalarFunction(TObjectName scalarFunctionName, [NotNullWhen(true)] out ScalarFunctionMetadata? scalarFunction)
	{
		try
		{
			scalarFunction = GetScalarFunction(scalarFunctionName);
			return true;
		}
		catch (MissingObjectException)
		{
			scalarFunction = null;
			return false;
		}
		catch (NotSupportedException)
		{
			scalarFunction = null;
			return false;
		}
	}

	/// <summary>
	/// Tries to get the stored procedure's metadata.
	/// </summary>
	/// <param name="procedureName">Name of the procedure.</param>
	/// <param name="storedProcedure">The stored procedure.</param>
	/// <returns></returns>
	public bool TryGetStoredProcedure(string procedureName, [NotNullWhen(true)] out StoredProcedureMetadata? storedProcedure) =>
		TryGetStoredProcedure(ParseObjectName(procedureName), out storedProcedure);

	/// <summary>
	/// Tries to get the stored procedure's metadata.
	/// </summary>
	/// <param name="procedureName">Name of the procedure.</param>
	/// <param name="storedProcedure">The stored procedure.</param>
	/// <returns></returns>
	public bool TryGetStoredProcedure(TObjectName procedureName, [NotNullWhen(true)] out StoredProcedureMetadata? storedProcedure)
	{
		try
		{
			storedProcedure = GetStoredProcedure(procedureName);
			return true;
		}
		catch (MissingObjectException)
		{
			storedProcedure = null;
			return false;
		}
		catch (NotSupportedException)
		{
			storedProcedure = null;
			return false;
		}
	}

	/// <summary>
	/// Tries to get the metadata for a table function.
	/// </summary>
	/// <param name="tableFunctionName">Name of the table function.</param>
	/// <param name="tableFunction">The table function.</param>
	/// <returns></returns>
	public bool TryGetTableFunction(string tableFunctionName, [NotNullWhen(true)] out TableFunctionMetadata? tableFunction) =>
		TryGetTableFunction(ParseObjectName(tableFunctionName), out tableFunction);

	/// <summary>
	/// Tries to get the metadata for a table function.
	/// </summary>
	/// <param name="tableFunctionName">Name of the table function.</param>
	/// <param name="tableFunction">The table function.</param>
	/// <returns></returns>
	public bool TryGetTableFunction(TObjectName tableFunctionName, [NotNullWhen(true)] out TableFunctionMetadata? tableFunction)
	{
		try
		{
			tableFunction = GetTableFunction(tableFunctionName);
			return true;
		}
		catch (MissingObjectException)
		{
			tableFunction = null;
			return false;
		}
		catch (NotSupportedException)
		{
			tableFunction = null;
			return false;
		}
	}

	/// <summary>
	/// Tries to get the metadata for a table or view.
	/// </summary>
	/// <param name="tableName">Name of the table or view.</param>
	/// <param name="tableOrView">The table or view.</param>
	/// <returns></returns>
	bool IDatabaseMetadataCache.TryGetTableOrView(string tableName, [NotNullWhen(true)] out TableOrViewMetadata? tableOrView)
	{
		if (TryGetTableOrView(ParseObjectName(tableName), out var result))
		{
			tableOrView = result;
			return true;
		}
		tableOrView = null;
		return false;
	}

	/// <summary>
	/// Tries to get the metadata for a table or view.
	/// </summary>
	/// <param name="tableName">Name of the table or view.</param>
	/// <param name="tableOrView">The table or view.</param>
	/// <returns></returns>
	public bool TryGetTableOrView(TObjectName tableName, [NotNullWhen(true)] out TableOrViewMetadata<TObjectName, TDbType>? tableOrView)
	{
		try
		{
			tableOrView = GetTableOrView(tableName);
			return true;
		}
		catch (MissingObjectException)
		{
			tableOrView = null;
			return false;
		}
		catch (NotSupportedException)
		{
			tableOrView = null;
			return false;
		}
	}

	/// <summary>
	/// Try to get the metadata for a user defined type.
	/// </summary>
	/// <param name="typeName">Name of the type.</param>
	/// <param name="userDefinedTableType">Type of the user defined table type.</param>
	/// <returns></returns>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public bool TryGetUserDefinedTableType(string typeName, [NotNullWhen(true)] out UserDefinedTableTypeMetadata? userDefinedTableType) =>
		TryGetUserDefinedTableType(ParseObjectName(typeName), out userDefinedTableType);

	/// <summary>
	/// Try to get the metadata for a user defined type.
	/// </summary>
	/// <param name="typeName">Name of the type.</param>
	/// <param name="userDefinedTableType">Type of the user defined.</param>
	/// <returns></returns>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public bool TryGetUserDefinedTableType(TObjectName typeName, [NotNullWhen(true)] out UserDefinedTableTypeMetadata? userDefinedTableType)
	{
		try
		{
			userDefinedTableType = GetUserDefinedTableType(typeName);
			return true;
		}
		catch (MissingObjectException)
		{
			userDefinedTableType = null;
			return false;
		}
		catch (NotSupportedException)
		{
			userDefinedTableType = null;
			return false;
		}
	}

	/// <summary>
	/// Removes a database type registration and its CLR equivalent. If a builtin type, this restores it to its default behavior.
	/// </summary>
	/// <param name="databaseTypeName">Name of the database type.</param>
	/// <remarks>True if the type was successfully unregistered. False if the type was not found.</remarks>
	public bool UnregisterType(string databaseTypeName)
	{
		return m_RegisteredTypes.TryRemove(databaseTypeName, out var _);
	}

	/// <summary>
	/// Converts a value to a string suitable for use in a SQL statement.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="dbType">Optional database column type.</param>
	/// <returns></returns>
	/// <remarks>Override this to support custom escaping logic.</remarks>
	[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "dbType")]
	[SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
	public virtual string ValueToSqlValue(object value, TDbType? dbType)
	{
		switch (value)
		{
			case DBNull _:
			case System.Reflection.Missing _:
			case null:
				return "NULL";

			case byte _:
			case sbyte _:
			case short _:
			case ushort _:
			case int _:
			case uint _:
			case long _:
			case ulong _:
			case decimal _:
			case float _:
			case double _:
				return value.ToString()!;

			case string s:
				return "'" + s.Replace("'", "''", StringComparison.Ordinal) + "'";

			case DateTime d:
				return "'" + d.ToString("O", CultureInfo.InvariantCulture) + "'"; //ISO 8601

			case DateTimeOffset d:
				return "'" + d.ToString("O", CultureInfo.InvariantCulture) + "'"; //ISO 8601

			case TimeSpan ts:
				return "'" + ts.ToString("hh:mm:ss.fffffff", CultureInfo.InvariantCulture) + "'"; //ISO 8601

			default:
				if (dbType.HasValue)
					throw new NotSupportedException($"Converting a value of type {value.GetType().Name} into a string of type {dbType.ToString()} is not supported. Try filing a bug report.");
				else
					throw new NotSupportedException($"Converting a value of type {value.GetType().Name} is not supported. Try supplying a dbType or filing a bug report.");
		}
	}

	/// <summary>
	/// Parse a string and return the database specific representation of the database object's name.
	/// </summary>
	/// <param name="schema"></param>
	/// <param name="name"></param>
	protected abstract TObjectName ParseObjectName(string? schema, string name);

	/// <summary>
	/// Determines the database column type from the column type name.
	/// </summary>
	/// <param name="typeName">Name of the database column type.</param>
	/// <param name="isUnsigned">Indicates whether or not the column is unsigned. Only applicable to some databases.</param>
	/// <returns></returns>
	/// <remarks>This does not honor registered types. This is only used for the database's hard-coded list of native types.</remarks>
	protected abstract TDbType? SqlTypeNameToDbType(string typeName, bool? isUnsigned);

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
	protected abstract Type? ToClrType(TDbType dbType, bool isNullable, int? maxLength);

	/// <summary>
	/// Tries the registered column type from a database column type name.
	/// </summary>
	/// <param name="databaseTypeName">Name of the database type.</param>
	/// <param name="registeredType">Type of the registered.</param>
	/// <returns></returns>
	protected bool TryGetRegisteredType(string databaseTypeName, [NotNullWhen(true)] out TypeRegistration<TDbType>? registeredType)
	{
		return m_RegisteredTypes.TryGetValue(databaseTypeName, out registeredType!);
	}
}
