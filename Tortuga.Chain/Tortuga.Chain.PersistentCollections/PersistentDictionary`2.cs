using System.Collections;
using Tortuga.Chain.CommandBuilders;
using Tortuga.Chain.DataSources;
using Tortuga.Chain.Metadata;

namespace Tortuga.Chain.PersistentCollections;

/// <summary>
/// PersistentDictionary is a table-backed dictionary. Any operations performed against this object will be resolved by the underlying database. This object is thread safe.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class PersistentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
	where TKey : notnull, IComparable<TKey>

{
	readonly ICrudDataSource m_DataSource;

	readonly string m_KeyColumnName;
	readonly TableOrViewMetadata m_Table;
	readonly string m_ValueColumnName;
	readonly string m_KeyFilter;
	readonly string m_ValueSql;
	readonly string m_BothFilter;

	/// <summary>
	/// Initializes a new instance of the <see cref="PersistentDictionary{TKey, TValue}"/> class.
	/// </summary>
	/// <param name="dataSource">The data source.</param>
	/// <param name="tableName">Name of the table.</param>
	/// <param name="keyColumnName">Name of the key column.</param>
	/// <param name="valueColumnName">Name of the value column.</param>
	/// <exception cref="MappingException">The table {tableName} doesn't have a column named {keyColumnName}</exception>
	/// <exception cref="MappingException">The table {tableName} doesn't have a column named {valueColumnName}</exception>
	internal PersistentDictionary(ICrudDataSource dataSource, string tableName, string keyColumnName, string valueColumnName)
	{
		m_DataSource = dataSource;
		m_Table = m_DataSource.DatabaseMetadata.GetTableOrView(tableName);

		var keyColumn = m_Table.Columns.TryGetColumn(keyColumnName);
		if (keyColumn == null)
			throw new MappingException($"The table {tableName} doesn't have a column named {keyColumnName}");
		m_KeyColumnName = keyColumn.SqlName;

		var valueColumn = m_Table.Columns.TryGetColumn(valueColumnName);
		if (valueColumn == null)
			throw new MappingException($"The table {tableName} doesn't have a column named {valueColumnName}");
		m_ValueColumnName = valueColumn.SqlName;

		m_KeyFilter = $"{keyColumn.QuotedSqlName} = @Key";
		m_ValueSql = $"{valueColumn.QuotedSqlName} = @Value";
		m_BothFilter = m_KeyFilter + "  AND " + m_ValueSql;
	}

	/// <summary>
	/// Gets the count.
	/// </summary>
	/// <value>The count.</value>
	public int Count => (int)m_DataSource.From(m_Table.Name).AsCount().Execute();

	/// <summary>
	/// Gets the is read only.
	/// </summary>
	/// <value>The is read only.</value>
	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

	/// <summary>
	/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
	/// </summary>
	/// <value>The keys.</value>
	public ICollection<TKey> Keys
	{
		get
		{
			return QueryList<TKey>(m_DataSource.From(m_Table.Name), m_KeyColumnName);
		}
	}

	/// <summary>
	/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
	/// </summary>
	/// <value>The values.</value>
	public ICollection<TValue> Values
	{
		get
		{
			return QueryList<TValue>(m_DataSource.From(m_Table.Name), m_ValueColumnName);
		}
	}

	ICollection<T> QueryList<T>(ITableDbCommandBuilder query, string columnName)
	{
		var type = typeof(T);

		if (type == typeof(string)) return (ICollection<T>)(object)query.ToStringList(m_ValueColumnName).Execute();
		if (type == typeof(char)) return (ICollection<T>)(object)query.ToCharList(m_ValueColumnName).Execute();
		if (type == typeof(short)) return (ICollection<T>)(object)query.ToInt16List(m_ValueColumnName).Execute();
		if (type == typeof(int)) return (ICollection<T>)(object)query.ToInt32List(m_ValueColumnName).Execute(); ;
		if (type == typeof(long)) return (ICollection<T>)(object)query.ToInt64List(m_ValueColumnName).Execute(); ;
		//if (type == typeof(ushort)) return (TValue)(object)query.ToUInt16List(m_ValueColumnName).Execute(); ;
		//if (type == typeof(uint)) return (TValue)(object)query.ToUInt32List(m_ValueColumnName).Execute();
		//if (type == typeof(ulong)) return (TValue)(object)query.ToUInt64List(m_ValueColumnName).Execute();
		if (type == typeof(float)) return (ICollection<T>)(object)query.ToSingleList(m_ValueColumnName).Execute();
		if (type == typeof(double)) return (ICollection<T>)(object)query.ToDoubleList(m_ValueColumnName).Execute();
		if (type == typeof(decimal)) return (ICollection<T>)(object)query.ToDecimalList(m_ValueColumnName).Execute();
		if (type == typeof(Guid)) return (ICollection<T>)(object)query.ToGuidList(m_ValueColumnName).Execute();
#if NET6_0_OR_GREATER
		//if (type == typeof(DateOnly)) return (TValue)(object)query.ToDateOnlyList(m_ValueColumnName).Execute();
		//if (type == typeof(TimeOnly)) return (TValue)(object)query.ToTimeOnlyList(m_ValueColumnName).Execute();
#endif
		if (type == typeof(DateTime)) return (ICollection<T>)(object)query.ToDateTimeList(m_ValueColumnName).Execute();
		if (type == typeof(TimeSpan)) return (ICollection<T>)(object)query.ToTimeSpanList(m_ValueColumnName).Execute();
		if (type == typeof(DateTimeOffset)) return (ICollection<T>)(object)query.ToDateTimeOffsetList(m_ValueColumnName).Execute();

		throw new NotSupportedException($"Cannot use a {typeof(PersistentDictionary<,>).Name} with a T of type {type.Name}");
	}

	/// <summary>
	/// Gets or sets the value with the specified key.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <returns>TValue.</returns>
	public TValue this[TKey key]
	{
		get
		{
			var type = typeof(TValue);
			var query = m_DataSource.From(m_Table.Name, m_KeyFilter, new { Key = key });

			if (type == typeof(string)) return (TValue)(object)query.ToString(m_ValueColumnName).Execute();
			if (type == typeof(char)) return (TValue)(object)query.ToChar(m_ValueColumnName).Execute();
			if (type == typeof(short)) return (TValue)(object)query.ToInt16(m_ValueColumnName).Execute();
			if (type == typeof(int)) return (TValue)(object)query.ToInt32(m_ValueColumnName).Execute(); ;
			if (type == typeof(long)) return (TValue)(object)query.ToInt64(m_ValueColumnName).Execute(); ;
			//if (type == typeof(ushort)) return (TValue)(object)query.ToUInt16(m_ValueColumnName).Execute(); ;
			//if (type == typeof(uint)) return (TValue)(object)query.ToUInt32(m_ValueColumnName).Execute();
			//if (type == typeof(ulong)) return (TValue)(object)query.ToUInt64(m_ValueColumnName).Execute();
			if (type == typeof(float)) return (TValue)(object)query.ToSingle(m_ValueColumnName).Execute();
			if (type == typeof(double)) return (TValue)(object)query.ToDouble(m_ValueColumnName).Execute();
			if (type == typeof(decimal)) return (TValue)(object)query.ToDecimal(m_ValueColumnName).Execute();
			if (type == typeof(Guid)) return (TValue)(object)query.ToGuid(m_ValueColumnName).Execute();
#if NET6_0_OR_GREATER
		//if (type == typeof(DateOnly)) return (TValue)(object)query.ToDateOnly(m_ValueColumnName).Execute();
		//if (type == typeof(TimeOnly)) return (TValue)(object)query.ToTimeOnly(m_ValueColumnName).Execute();
#endif
			if (type == typeof(DateTime)) return (TValue)(object)query.ToDateTime(m_ValueColumnName).Execute();
			if (type == typeof(TimeSpan)) return (TValue)(object)query.ToTimeSpan(m_ValueColumnName).Execute();
			if (type == typeof(DateTimeOffset)) return (TValue)(object)query.ToDateTimeOffset(m_ValueColumnName).Execute();

			throw new NotSupportedException($"Cannot use a {typeof(PersistentDictionary<,>).Name} with a TValue of type {type.Name}");
		}
		set => m_DataSource.UpdateSet(m_Table.Name, m_ValueSql, new { Value = value }).WithFilter(m_KeyFilter, new { Key = key }).Execute();
	}

	/// <summary>
	/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
	/// </summary>
	/// <param name="key">The object to use as the key of the element to add.</param>
	/// <param name="value">The object to use as the value of the element to add.</param>
	public void Add(TKey key, TValue value)
	{
		m_DataSource.Insert(m_Table.Name, new Dictionary<string, object?> { { m_KeyColumnName, key }, { m_ValueColumnName, value } }).Execute();
	}

	/// <summary>
	/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
	/// </summary>
	/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	/// <summary>
	/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
	/// </summary>
	public void Clear()
	{
		if (m_DataSource is ISupportsTruncate st)
			st.Truncate(m_Table.Name).Execute();
		else
			m_DataSource.DeleteAll(m_Table.Name).Execute();
	}

	/// <summary>
	/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
	/// </summary>
	/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
	/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
	/// </summary>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
	/// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.</returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool ContainsKey(TKey key)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
	/// </summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="NotImplementedException"></exception>
	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	/// <exception cref="NotImplementedException"></exception>
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		throw new NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
	/// </summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
	public bool Remove(TKey key)
	{
		//AsNonQuery returns the number of affected rows.
		return (m_DataSource.DeleteSet(m_Table.Name, m_KeyFilter, new { Key = key }).AsNonQuery().Execute() ?? 0) > 0;
	}

	/// <summary>
	/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
	/// </summary>
	/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
	/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return (m_DataSource.DeleteSet(m_Table.Name, m_BothFilter, new { item.Key, item.Value }).AsNonQuery().Execute() ?? 0) > 0;
	}

	/// <summary>
	/// Gets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key whose value to get.</param>
	/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
	/// <returns>true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <exception cref="NotImplementedException"></exception>
	public bool TryGetValue(TKey key, out TValue value)
	{
		throw new NotImplementedException();
	}
}
