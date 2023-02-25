using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Tortuga.Chain.CommandBuilders;

/// <summary>
/// This allows the use of multi-row materializers against a command builder.
/// </summary>
/// <remarks>Warning: This interface is meant to simulate multiple inheritance and work-around some issues with exposing generic types. Do not implement it in client code, as new method will be added over time.</remarks>
public interface IMultipleRowDbCommandBuilder : ISingleRowDbCommandBuilder
{
	/// <summary>
	/// Indicates the results should be materialized as a list of booleans.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<bool>> ToBooleanList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of booleans.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<bool>> ToBooleanList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of booleans.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<bool?>> ToBooleanOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of booleans.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<bool?>> ToBooleanOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of byte arrays.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<byte[]>> ToByteArrayList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of byte arrays.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<byte[]>> ToByteArrayList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of byte arrays.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<byte[]?>> ToByteArrayOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of byte arrays.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<byte[]?>> ToByteArrayOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of chars.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<char>> ToCharList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of chars.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<char>> ToCharList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of chars.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns>Tortuga.Chain.ILink&lt;System.Collections.Generic.List&lt;System.Char&gt;&gt;.</returns>
	ILink<List<char?>> ToCharOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of chars.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns>Tortuga.Chain.ILink&lt;System.Collections.Generic.List&lt;System.Char&gt;&gt;.</returns>
	ILink<List<char?>> ToCharOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Materializes the result as a list of objects.
	/// </summary>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <param name="collectionOptions">The collection options.</param>
	/// <returns></returns>
	IConstructibleMaterializer<List<TObject>> ToCollection<TObject>(CollectionOptions collectionOptions = CollectionOptions.None)
		where TObject : class;

	/// <summary>
	/// Materializes the result as a list of objects.
	/// </summary>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <typeparam name="TCollection">The type of the collection.</typeparam>
	/// <param name="collectionOptions">The collection options.</param>
	/// <returns></returns>
	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	IConstructibleMaterializer<TCollection> ToCollection<TObject, TCollection>(CollectionOptions collectionOptions = CollectionOptions.None)
		where TObject : class
		where TCollection : ICollection<TObject>, new();

	/// <summary>
	/// Indicates the results should be materialized as a DataSet.
	/// </summary>
	ILink<DataTable> ToDataTable();

	/// <summary>
	/// Indicates the results should be materialized as a list of DateTime.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<DateTime>> ToDateTimeList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of DateTime.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<DateTime>> ToDateTimeList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of DateTimeOffset.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<DateTimeOffset>> ToDateTimeOffsetList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of DateTimeOffset.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<DateTimeOffset>> ToDateTimeOffsetList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of DateTimeOffset.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<DateTimeOffset?>> ToDateTimeOffsetOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of DateTimeOffset.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<DateTimeOffset?>> ToDateTimeOffsetOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of DateTimeOffset.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<DateTimeOffset>> ToDateTimeOffsetSet(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of DateTimeOffset.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<DateTimeOffset>> ToDateTimeOffsetSet(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of DateTime.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<DateTime?>> ToDateTimeOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of DateTime.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<DateTime?>> ToDateTimeOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of DateTime.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<DateTime>> ToDateTimeSet(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of DateTime.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<DateTime>> ToDateTimeSet(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<decimal>> ToDecimalList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<decimal>> ToDecimalList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<decimal?>> ToDecimalOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<decimal?>> ToDecimalOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<decimal>> ToDecimalSet(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<decimal>> ToDecimalSet(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Materializes the result as a dictionary of objects.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <param name="keyColumn">The key column.</param>
	/// <param name="dictionaryOptions">The dictionary options.</param>
	/// <returns>IConstructibleMaterializer&lt;Dictionary&lt;TKey, TObject&gt;&gt;.</returns>
	IConstructibleMaterializer<Dictionary<TKey, TObject>> ToDictionary<TKey, TObject>(string keyColumn, DictionaryOptions dictionaryOptions = DictionaryOptions.None)
		where TKey : notnull
		where TObject : class;

	/// <summary>
	/// Materializes the result as a dictionary of objects.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <typeparam name="TDictionary">The type of dictionary.</typeparam>
	/// <param name="keyColumn">The key column.</param>
	/// <param name="dictionaryOptions">The dictionary options.</param>
	/// <returns>IConstructibleMaterializer&lt;TDictionary&gt;.</returns>
	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	IConstructibleMaterializer<TDictionary> ToDictionary<TKey, TObject, TDictionary>(string keyColumn, DictionaryOptions dictionaryOptions = DictionaryOptions.None)
		where TKey : notnull
		where TObject : class
		where TDictionary : IDictionary<TKey, TObject>, new();

	/// <summary>
	/// Materializes the result as a dictionary of objects.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <param name="keyFunction">The key function.</param>
	/// <param name="dictionaryOptions">The dictionary options.</param>
	/// <returns>IConstructibleMaterializer&lt;Dictionary&lt;TKey, TObject&gt;&gt;.</returns>
	IConstructibleMaterializer<Dictionary<TKey, TObject>> ToDictionary<TKey, TObject>(Func<TObject, TKey> keyFunction, DictionaryOptions dictionaryOptions = DictionaryOptions.None)
		where TKey : notnull
		where TObject : class;

	/// <summary>
	/// Materializes the result as a dictionary of objects.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <typeparam name="TDictionary">The type of dictionary.</typeparam>
	/// <param name="keyFunction">The key function.</param>
	/// <param name="dictionaryOptions">The dictionary options.</param>
	/// <returns>IConstructibleMaterializer&lt;TDictionary&gt;.</returns>
	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	IConstructibleMaterializer<TDictionary> ToDictionary<TKey, TObject, TDictionary>(Func<TObject, TKey> keyFunction, DictionaryOptions dictionaryOptions = DictionaryOptions.None)
		where TKey : notnull
		where TObject : class
		where TDictionary : IDictionary<TKey, TObject>, new();

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<double>> ToDoubleList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<double>> ToDoubleList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<double?>> ToDoubleOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<double?>> ToDoubleOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<double>> ToDoubleSet(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<double>> ToDoubleSet(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Materializes the result as a list of dynamically typed objects.
	/// </summary>
	/// <returns></returns>
	IColumnSelectingMaterializer<List<dynamic>> ToDynamicCollection();

	/// <summary>
	/// Indicates the results should be materialized as a list of Guids.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<Guid>> ToGuidList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of Guids.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<Guid>> ToGuidList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of Guids.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<Guid?>> ToGuidOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of Guids.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<Guid?>> ToGuidOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of Guids.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<Guid>> ToGuidSet(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of Guids.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<Guid>> ToGuidSet(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Materializes the result as an immutable array of objects.
	/// </summary>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <param name="collectionOptions">The collection options.</param>
	/// <returns>Tortuga.Chain.IConstructibleMaterializer&lt;System.Collections.Immutable.ImmutableArray&lt;TObject&gt;&gt;.</returns>
	/// <exception cref="MappingException"></exception>
	/// <remarks>In theory this will offer better performance than ToImmutableList if you only intend to read the result.</remarks>
	IConstructibleMaterializer<ImmutableArray<TObject>> ToImmutableArray<TObject>(CollectionOptions collectionOptions = CollectionOptions.None)
   where TObject : class;

	/// <summary>
	/// Materializes the result as a immutable dictionary of objects.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <param name="keyFunction">The key function.</param>
	/// <param name="dictionaryOptions">The dictionary options.</param>
	/// <returns></returns>
	IConstructibleMaterializer<ImmutableDictionary<TKey, TObject>> ToImmutableDictionary<TKey, TObject>(Func<TObject, TKey> keyFunction, DictionaryOptions dictionaryOptions = DictionaryOptions.None)
		where TKey : notnull
		where TObject : class;

	/// <summary>
	/// Materializes the result as a immutable dictionary of objects.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <param name="keyColumn">The key column.</param>
	/// <param name="dictionaryOptions">The dictionary options.</param>
	/// <returns></returns>
	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	IConstructibleMaterializer<ImmutableDictionary<TKey, TObject>> ToImmutableDictionary<TKey, TObject>(string keyColumn, DictionaryOptions dictionaryOptions = DictionaryOptions.None)
		where TKey : notnull
		where TObject : class;

	/// <summary>
	/// Materializes the result as an immutable list of objects.
	/// </summary>
	/// <typeparam name="TObject">The type of the model.</typeparam>
	/// <param name="collectionOptions">The collection options.</param>
	/// <returns>Tortuga.Chain.IConstructibleMaterializer&lt;System.Collections.Immutable.ImmutableList&lt;TObject&gt;&gt;.</returns>
	/// <exception cref="MappingException"></exception>
	/// <remarks>In theory this will offer better performance than ToImmutableArray if you intend to further modify the result.</remarks>
	IConstructibleMaterializer<ImmutableList<TObject>> ToImmutableList<TObject>(CollectionOptions collectionOptions = CollectionOptions.None)
   where TObject : class;

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<short>> ToInt16List(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<short>> ToInt16List(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<short?>> ToInt16OrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<short?>> ToInt16OrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<short>> ToInt16Set(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<short>> ToInt16Set(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<int>> ToInt32List(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<int>> ToInt32List(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<int?>> ToInt32OrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<int?>> ToInt32OrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<int>> ToInt32Set(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<int>> ToInt32Set(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<long>> ToInt64List(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<long>> ToInt64List(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<long?>> ToInt64OrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<long?>> ToInt64OrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of integers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<long>> ToInt64Set(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of integers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<long>> ToInt64Set(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<float>> ToSingleList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<float>> ToSingleList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<float?>> ToSingleOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<float?>> ToSingleOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of numbers.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<float>> ToSingleSet(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of numbers.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<float>> ToSingleSet(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of strings.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<string>> ToStringList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of strings.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<string>> ToStringList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of strings.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<string?>> ToStringOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of strings.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<string?>> ToStringOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of strings.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<string>> ToStringSet(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of strings.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<string>> ToStringSet(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a Table.
	/// </summary>
	ILink<Table> ToTable();

	/// <summary>
	/// Indicates the results should be materialized as a list of TimeSpan.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<TimeSpan>> ToTimeSpanList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of TimeSpan.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<TimeSpan>> ToTimeSpanList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of TimeSpan.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<TimeSpan?>> ToTimeSpanOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a list of TimeSpan.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<List<TimeSpan?>> ToTimeSpanOrNullList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of TimeSpan.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<TimeSpan>> ToTimeSpanSet(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Indicates the results should be materialized as a set of TimeSpan.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns></returns>
	ILink<HashSet<TimeSpan>> ToTimeSpanSet(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Materializes the result as a list of XElement.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns>Tortuga.Chain.ILink&lt;System.Collections.Generic.List&lt;XDocument&gt;&gt;.</returns>
	ILink<List<XElement>> ToXmlList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Materializes the result as a list of XElement.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns>Tortuga.Chain.ILink&lt;System.Collections.Generic.List&lt;XDocument&gt;&gt;.</returns>
	ILink<List<XElement>> ToXmlList(string columnName, ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Materializes the result as a list of XElement.
	/// </summary>
	/// <param name="listOptions">The list options.</param>
	/// <returns>Tortuga.Chain.ILink&lt;System.Collections.Generic.List&lt;XDocument&gt;&gt;.</returns>
	ILink<List<XElement?>> ToXmlOrNullList(ListOptions listOptions = ListOptions.None);

	/// <summary>
	/// Materializes the result as a list of XElement.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <param name="listOptions">The list options.</param>
	/// <returns>Tortuga.Chain.ILink&lt;System.Collections.Generic.List&lt;XDocument&gt;&gt;.</returns>
	ILink<List<XElement?>> ToXmlOrNullList(string columnName, ListOptions listOptions = ListOptions.None);
}
