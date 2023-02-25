using System.Diagnostics.CodeAnalysis;
using Tortuga.Chain.Aggregates;

namespace Tortuga.Chain.Metadata;

/// <summary>
/// Abstract version of the database metadata cache.
/// </summary>
public interface IDatabaseMetadataCache
{
	/// <summary>
	/// Gets the converter dictionary used by materializers.
	/// </summary>
	/// <value>The converter dictionary.</value>
	MaterializerTypeConverter Converter { get; }

	/// <summary>
	/// Gets the maximum number of parameters in a single SQL batch.
	/// </summary>
	/// <value>The maximum number of parameters.</value>
	int? MaxParameters { get; }

	/// <summary>
	/// Gets an aggregate function.
	/// </summary>
	/// <param name="aggregateType">Type of the aggregate.</param>
	/// <param name="columnName">Name of the column to insert into the function.</param>
	/// <returns>A string suitable for use in an aggregate.</returns>
	string GetAggregateFunction(AggregateType aggregateType, string columnName);

	/// <summary>
	/// Gets the stored procedure's metadata.
	/// </summary>
	/// <param name="procedureName">Name of the procedure.</param>
	/// <returns></returns>
	StoredProcedureMetadata GetStoredProcedure(string procedureName);

	/// <summary>
	/// Gets the stored procedures that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Call Preload before invoking this method to ensure that all stored procedures were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.</remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	IReadOnlyCollection<StoredProcedureMetadata> GetStoredProcedures();

	/// <summary>
	/// Gets the metadata for a table function.
	/// </summary>
	/// <param name="tableFunctionName">Name of the table function.</param>
	TableFunctionMetadata GetTableFunction(string tableFunctionName);

	/// <summary>
	/// Gets the table-valued functions that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Call Preload before invoking this method to ensure that all table-valued functions were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.</remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	IReadOnlyCollection<TableFunctionMetadata> GetTableFunctions();

	/// <summary>
	/// Gets the metadata for a table or view.
	/// </summary>
	/// <param name="tableName">Name of the table or view.</param>
	/// <returns></returns>
	TableOrViewMetadata GetTableOrView(string tableName);

	/// <summary>
	/// Returns the table or view derived from the class's name and/or Table attribute.
	/// </summary>
	/// <typeparam name="TObject"></typeparam>
	/// <param name="operation"></param>
	/// <returns></returns>
	[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
	TableOrViewMetadata GetTableOrViewFromClass<TObject>(OperationType operation = OperationType.All);

	/// <summary>
	/// Returns the table or view derived from the class's name and/or Table attribute.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="operation"></param>
	/// <returns></returns>
	TableOrViewMetadata GetTableOrViewFromClass(Type type, OperationType operation = OperationType.All);

	/// <summary>
	/// Gets the tables and views that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Call Preload before invoking this method to ensure that all tables and views were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.</remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	IReadOnlyCollection<TableOrViewMetadata> GetTablesAndViews();

	/// <summary>
	/// Gets the metadata for a user defined type.
	/// </summary>
	/// <param name="typeName">Name of the type.</param>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	UserDefinedTableTypeMetadata GetUserDefinedTableType(string typeName);

	/// <summary>
	/// Gets the user defined table types that were loaded by this cache.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Call Preload before invoking this method to ensure that all table-valued functions were loaded from the database's schema. Otherwise only the objects that were actually used thus far will be returned.</remarks>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	IReadOnlyCollection<UserDefinedTableTypeMetadata> GetUserDefinedTableTypes();

	/// <summary>
	/// Preloads all of the metadata for this data source.
	/// </summary>
	void Preload();

	///// <summary>
	///// Preloads all of the tables for this data source.
	///// </summary>
	//void PreloadTables();

	///// <summary>
	///// Preloads all of the views for this data source.
	///// </summary>
	//void PreloadViews();

	/// <summary>
	/// Quotes the name of the column for use in SQL generation.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <remarks>This will not quote a "*". If the name starts with the quote character, it assumes the name is already quoted.</remarks>
	abstract string QuoteColumnName(string columnName);

	/// <summary>
	/// Quotes the name of a table, view, etc. for use in SQL generation.
	/// </summary>
	/// <param name="objectName">Name of the object.</param>
	abstract string QuoteObjectName(string objectName);

	/// <summary>
	/// Resets the metadata cache, clearing out all cached metadata.
	/// </summary>
	void Reset();

	/// <summary>
	/// Tries to get the stored procedure's metadata.
	/// </summary>
	/// <param name="procedureName">Name of the procedure.</param>
	/// <param name="storedProcedure">The stored procedure.</param>
	/// <returns></returns>
	bool TryGetStoredProcedure(string procedureName, [NotNullWhen(true)] out StoredProcedureMetadata? storedProcedure);

	/// <summary>
	/// Tries to get the metadata for a table function.
	/// </summary>
	/// <param name="tableFunctionName">Name of the table function.</param>
	/// <param name="tableFunction">The table function.</param>
	/// <returns></returns>
	bool TryGetTableFunction(string tableFunctionName, [NotNullWhen(true)] out TableFunctionMetadata? tableFunction);

	/// <summary>
	/// Tries to get the metadata for a table or view.
	/// </summary>
	/// <param name="tableName">Name of the table or view.</param>
	/// <param name="tableOrView">The table or view.</param>
	/// <returns></returns>
	bool TryGetTableOrView(string tableName, [NotNullWhen(true)] out TableOrViewMetadata? tableOrView);

	/// <summary>
	/// Try to get the metadata for a user defined type.
	/// </summary>
	/// <param name="typeName">Name of the type.</param>
	/// <param name="userDefinedTableType">Type of the user defined table type.</param>
	/// <returns></returns>
	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	bool TryGetUserDefinedTableType(string typeName, [NotNullWhen(true)] out UserDefinedTableTypeMetadata? userDefinedTableType);
}
