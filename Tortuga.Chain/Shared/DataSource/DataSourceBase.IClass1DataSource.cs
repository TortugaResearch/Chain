﻿using Tortuga.Chain.CommandBuilders;
using Tortuga.Chain.DataSources;

#if SQL_SERVER_SDS || SQL_SERVER_MDS

namespace Tortuga.Chain.SqlServer
{
	partial class SqlServerDataSourceBase : ICrudDataSource
#elif SQL_SERVER_OLEDB

namespace Tortuga.Chain.SqlServer
{
    partial class OleDbSqlServerDataSourceBase : ICrudDataSource

#elif SQLITE

namespace Tortuga.Chain.SQLite
{
    partial class SQLiteDataSourceBase : ICrudDataSource

#elif MYSQL

namespace Tortuga.Chain.MySql
{
	partial class MySqlDataSourceBase : ICrudDataSource

#elif POSTGRESQL

namespace Tortuga.Chain.PostgreSql
{
    partial class PostgreSqlDataSourceBase : ICrudDataSource

#elif ACCESS

namespace Tortuga.Chain.Access
{
	partial class AccessDataSourceBase : ICrudDataSource

#endif
	{
		IObjectDbCommandBuilder<TArgument> ISupportsDelete.Delete<TArgument>(string tableName, TArgument argumentValue, DeleteOptions options)
		{
			return Delete(tableName, argumentValue, options);
		}

		IObjectDbCommandBuilder<TArgument> ISupportsDelete.Delete<TArgument>(TArgument argumentValue, DeleteOptions options)
		{
			return Delete(argumentValue, options);
		}

		ISingleRowDbCommandBuilder ISupportsDeleteByKey.DeleteByKey<TKey>(string tableName, TKey key, DeleteOptions options)
		{
			return DeleteByKey(tableName, key, options);
		}

		ISingleRowDbCommandBuilder ISupportsDeleteByKey.DeleteByKey(string tableName, string key, DeleteOptions options)
		{
			return DeleteByKey(tableName, key, options);
		}

		IMultipleRowDbCommandBuilder ISupportsDeleteByKeyList.DeleteByKeyList<TKey>(string tableName, IEnumerable<TKey> keys, DeleteOptions options)
		{
			return DeleteByKeyList(tableName, keys, options);
		}

		IMultipleRowDbCommandBuilder ISupportsDeleteWithFilter.DeleteWithFilter(string tableName, string whereClause)
		{
			return DeleteWithFilter(tableName, whereClause);
		}

		IMultipleRowDbCommandBuilder ISupportsDeleteWithFilter.DeleteWithFilter(string tableName, string whereClause, object argumentValue)
		{
			return DeleteWithFilter(tableName, whereClause, argumentValue);
		}

		IMultipleRowDbCommandBuilder ISupportsDeleteWithFilter.DeleteWithFilter(string tableName, object filterValue, FilterOptions filterOptions)
		{
			return DeleteWithFilter(tableName, filterValue, filterOptions);
		}

		ITableDbCommandBuilder ISupportsFrom.From(string tableOrViewName)
		{
			return From(tableOrViewName);
		}

		ITableDbCommandBuilder ISupportsFrom.From(string tableOrViewName, object filterValue, FilterOptions filterOptions)
		{
			return From(tableOrViewName, filterValue, filterOptions);
		}

		ITableDbCommandBuilder ISupportsFrom.From(string tableOrViewName, string whereClause)
		{
			return From(tableOrViewName, whereClause);
		}

		ITableDbCommandBuilder ISupportsFrom.From(string tableOrViewName, string whereClause, object argumentValue)
		{
			return From(tableOrViewName, whereClause, argumentValue);
		}

		ITableDbCommandBuilder<TObject> ISupportsFrom.From<TObject>()
		{
			return From<TObject>();
		}

		ITableDbCommandBuilder<TObject> ISupportsFrom.From<TObject>(string whereClause)
		{
			return From<TObject>(whereClause);
		}

		ITableDbCommandBuilder<TObject> ISupportsFrom.From<TObject>(string whereClause, object argumentValue)
		{
			return From<TObject>(whereClause, argumentValue);
		}

		ITableDbCommandBuilder<TObject> ISupportsFrom.From<TObject>(object filterValue)
		{
			return From<TObject>(filterValue);
		}

		ISingleRowDbCommandBuilder ISupportsGetByKey.GetByKey<TKey>(string tableName, TKey key)
		{
			return GetByKey(tableName, key);
		}

		ISingleRowDbCommandBuilder ISupportsGetByKey.GetByKey(string tableName, string key)
		{
			return GetByKey(tableName, key);
		}

		IMultipleRowDbCommandBuilder ISupportsGetByKeyList.GetByKeyList<T>(string tableName, string keyColumn, IEnumerable<T> keys)
		{
			return GetByKeyList(tableName, keyColumn, keys);
		}

		IMultipleRowDbCommandBuilder ISupportsGetByKeyList.GetByKeyList<T>(string tableName, IEnumerable<T> keys)
		{
			return GetByKeyList(tableName, keys);
		}

		IObjectDbCommandBuilder<TArgument> ISupportsInsert.Insert<TArgument>(string tableName, TArgument argumentValue, InsertOptions options)
		{
			return Insert(tableName, argumentValue, options);
		}

		IObjectDbCommandBuilder<TArgument> ISupportsInsert.Insert<TArgument>(TArgument argumentValue, InsertOptions options)
		{
			return Insert(argumentValue, options);
		}

		IObjectDbCommandBuilder<TArgument> ISupportsUpdate.Update<TArgument>(string tableName, TArgument argumentValue, UpdateOptions options)
		{
			return Update(tableName, argumentValue, options);
		}

		IObjectDbCommandBuilder<TArgument> ISupportsUpdate.Update<TArgument>(TArgument argumentValue, UpdateOptions options)
		{
			return Update(argumentValue, options);
		}

		ISingleRowDbCommandBuilder ISupportsUpdateByKey.UpdateByKey<TArgument, TKey>(string tableName, TArgument newValues, TKey key, UpdateOptions options)
		{
			return UpdateByKey(tableName, newValues, key, options);
		}

		ISingleRowDbCommandBuilder ISupportsUpdateByKey.UpdateByKey<TArgument>(string tableName, TArgument newValues, string key, UpdateOptions options)
		{
			return UpdateByKey(tableName, newValues, key, options);
		}

		IMultipleRowDbCommandBuilder ISupportsUpdateByKeyList.UpdateByKeyList<TArgument, TKey>(string tableName, TArgument newValues, IEnumerable<TKey> keys, UpdateOptions options)
		{
			return UpdateByKeyList(tableName, newValues, keys, options);
		}

		IUpdateManyDbCommandBuilder ISupportsUpdateSet.UpdateSet(string tableName, string updateExpression, UpdateOptions options)
		{
			return UpdateSet(tableName, updateExpression, options);
		}

		IUpdateManyDbCommandBuilder ISupportsUpdateSet.UpdateSet(string tableName, string updateExpression, object updateArgumentValue, UpdateOptions options)
		{
			return UpdateSet(tableName, updateExpression, updateArgumentValue, options);
		}

		IUpdateManyDbCommandBuilder ISupportsUpdateSet.UpdateSet(string tableName, object newValues, UpdateOptions options)
		{
			return UpdateSet(tableName, newValues, options);
		}
	}
}
