using Tortuga.Chain.DataSources;

namespace Tortuga.Chain.PersistentCollections;

/// <summary>
/// PersistentDictionary is a table-backed dictionary. Any operations performed against this object will be resolved by the underlying database.
/// </summary>
public struct PersistentDictionary<TDataSource>
	where TDataSource : IRootDataSource, ICrudDataSource
{
	readonly TDataSource m_DataSource;

	/// <summary>
	/// Initializes a new instance of the <see cref="PersistentDictionary{TDataSource}"/> struct.
	/// </summary>
	/// <param name="dataSource">The data source.</param>
	public PersistentDictionary(TDataSource dataSource)
	{
		m_DataSource = dataSource;
	}

	/// <summary>
	/// Creates the specified presistent dictionary, including it's underlying table.
	/// </summary>
	/// <typeparam name="TKey">The type of the key column.</typeparam>
	/// <typeparam name="TValue">The type of the value column.</typeparam>
	/// <param name="tableName">Name of the table.</param>
	/// <param name="options">The table creation options.</param>
	/// <exception cref="ArgumentException">A table named {tableName} already exists.</exception>
	/// <remarks>This will throw an error if the table already exists.</remarks>
	public PersistentDictionary<TKey, TValue> Create<TKey, TValue>(string tableName, PersistentDictionaryOptions? options = null)
		where TKey : notnull, IComparable<TKey>
	{
		var databaseMetadata = m_DataSource.DatabaseMetadata;

		if (databaseMetadata.TryGetTableOrView(tableName, out var table))
			throw new ArgumentException($"A table named {table.Name} already exists.");

		options = options ?? new();
		var safeTableName = databaseMetadata.QuoteObjectName(tableName);

		//TODO Create the table
		var sql = $@"CREATE TABLE {safeTableName}
			(
				{databaseMetadata.QuoteColumnName(options.KeyColumnName)} {databaseMetadata.ClrTypeToSqlTypeName<TKey>()} PRIMARY KEY,
				{databaseMetadata.QuoteColumnName(options.ValueColumnName)} {databaseMetadata.ClrTypeToSqlTypeName<TKey>()} NOT NULL
			)";
		m_DataSource.Sql(sql).Execute();

		return new PersistentDictionary<TKey, TValue>(m_DataSource, tableName, options.KeyColumnName, options.ValueColumnName);
	}

	/// <summary>
	/// Creates a presistent dictionary, including it's underlying table, if it doesn't exist.
	/// </summary>
	/// <typeparam name="TKey">The type of the key column.</typeparam>
	/// <typeparam name="TValue">The type of the value column.</typeparam>
	/// <param name="tableName">Name of the table.</param>
	/// <param name="options">The table creation options.</param>
	/// <remarks>This will not modify an exisitng table's schema.</remarks>
	/// <exception cref="MappingException">A table name {tableName} already exists, but it doesn't have a column named {keyColumnName}</exception>
	/// <exception cref="MappingException">A table name {tableName} already exists, but it doesn't have a column named {valueColumnName}</exception>
	public PersistentDictionary<TKey, TValue> CreateOrGet<TKey, TValue>(string tableName, PersistentDictionaryOptions? options = null)
		where TKey : notnull, IComparable<TKey>
	{
		var databaseMetadata = m_DataSource.DatabaseMetadata;
		options = options ?? new();

		if (databaseMetadata.TryGetTableOrView(tableName, out var table))
			return Get<TKey, TValue>(table.Name, options.KeyColumnName, options.ValueColumnName);

		return Create<TKey, TValue>(tableName, options);
	}

	/// <summary>
	/// Gets the specified presistent dictionary.
	/// </summary>
	/// <typeparam name="TKey">The type of the key column.</typeparam>
	/// <typeparam name="TValue">The type of the value column.</typeparam>
	/// <param name="tableName">Name of the table.</param>
	/// <param name="keyColumnName">Name of the key column.</param>
	/// <param name="valueColumnName">Name of the value column.</param>
	/// <exception cref="ArgumentException">A table named {tableName} doesn't exists.</exception>
	/// <exception cref="MappingException">A table name {tableName} already exists, but it doesn't have a column named {keyColumnName}</exception>
	/// <exception cref="MappingException">A table name {tableName} already exists, but it doesn't have a column named {valueColumnName}</exception>
	public PersistentDictionary<TKey, TValue> Get<TKey, TValue>(string tableName, string keyColumnName = "KeyColumn", string valueColumnName = "ValueColumn")
		where TKey : notnull, IComparable<TKey>
	{
		if (!m_DataSource.DatabaseMetadata.TryGetTableOrView(tableName, out var table))
			throw new ArgumentException($"A table named {tableName} doesn't exists.");

		var keyColumn = table.Columns.TryGetColumn(keyColumnName);
		if (keyColumn == null)
			throw new MappingException($"A table name {tableName} already exists, but it doesn't have a column named {keyColumnName}");
		var valueColumn = table.Columns.TryGetColumn(valueColumnName);
		if (valueColumn == null)
			throw new MappingException($"A table name {tableName} already exists, but it doesn't have a column named {valueColumnName}");

		return new PersistentDictionary<TKey, TValue>(m_DataSource, tableName, keyColumnName, valueColumnName);
	}
}
