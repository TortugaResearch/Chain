﻿using System;
using System.Collections.Generic;
using Tortuga.Chain.CommandBuilders;

namespace Tortuga.Chain
{
    /// <summary>
    /// A class 1 data source supports basic CRUD operations. This is the bare minimum needed to implement the repostiory pattern.
    /// </summary>
    /// <remarks>Warning: This interface is meant to simulate multiple inheritance and work-around some issues with exposing generic types. Do not implement it in client code, as new methods will be added over time.</remarks>
    public interface IClass1DataSource : IClass0DataSource
    {
        /// <summary>
        /// Delete an object model from the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The delete options.</param>
        /// <exception cref="ArgumentException">tableName is empty.;tableName</exception>
        IObjectDbCommandBuilder<TArgument> Delete<TArgument>(string tableName, TArgument argumentValue, DeleteOptions options = DeleteOptions.None) where TArgument : class;

        /// <summary>
        /// Delete an object model from the table indicated by the class's Table attribute.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The delete options.</param>
        /// <exception cref="ArgumentException">tableName is empty.;tableName</exception>
        IObjectDbCommandBuilder<TArgument> Delete<TArgument>(TArgument argumentValue, DeleteOptions options = DeleteOptions.None) where TArgument : class;

        /// <summary>
        /// Delete by key.
        /// </summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="key">The key.</param>
        /// <param name="options">The options.</param>
        /// <returns>ISingleRowDbCommandBuilder.</returns>
        ISingleRowDbCommandBuilder DeleteByKey<TKey>(string tableName, TKey key, DeleteOptions options = DeleteOptions.None) where TKey : struct;

        /// <summary>
        /// Delete by key.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="key">The key.</param>
        /// <param name="options">The options.</param>
        /// <returns>ISingleRowDbCommandBuilder.</returns>
        ISingleRowDbCommandBuilder DeleteByKey(string tableName, string key, DeleteOptions options = DeleteOptions.None);

        /// <summary>
        /// Delete by key.
        /// </summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="options">The options.</param>
        /// <returns>IMultipleRowDbCommandBuilder.</returns>
        IMultipleRowDbCommandBuilder DeleteByKeyList<TKey>(string tableName, IEnumerable<TKey> keys, DeleteOptions options = DeleteOptions.None);

        /// <summary>
        /// Delete multiple records using a where expression.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns>IMultipleRowDbCommandBuilder.</returns>
        IMultipleRowDbCommandBuilder DeleteWithFilter(string tableName, string whereClause);

        /// <summary>
        /// Delete multiple records using a where expression.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="argumentValue">The argument value for the where clause.</param>
        /// <returns>IMultipleRowDbCommandBuilder.</returns>
        IMultipleRowDbCommandBuilder DeleteWithFilter(string tableName, string whereClause, object argumentValue);

        /// <summary>
        /// Delete multiple records using a filter object.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <param name="filterOptions">The filter options.</param>
        IMultipleRowDbCommandBuilder DeleteWithFilter(string tableName, object filterValue, FilterOptions filterOptions = FilterOptions.None);

        /// <summary>
        /// This is used to directly query a table or view.
        /// </summary>
        /// <param name="tableOrViewName">Name of the table or view.</param>
        /// <exception cref="ArgumentException">
        /// tableName is empty.;tableName
        /// or
        /// Table or view named + tableName +  could not be found. Check to see if the user has permissions to execute this procedure.
        /// </exception>
        ITableDbCommandBuilder From(string tableOrViewName);

        /// <summary>
        /// This is used to directly query a table or view.
        /// </summary>
        /// <param name="tableOrViewName">Name of the table or view.</param>
        /// <param name="whereClause">The where clause. Do not prefix this clause with "WHERE".</param>
        /// <exception cref="ArgumentException">tableOrViewName is empty.;tableOrViewName</exception>
        ITableDbCommandBuilder From(string tableOrViewName, string whereClause);

        /// <summary>
        /// This is used to directly query a table or view.
        /// </summary>
        /// <param name="tableOrViewName">Name of the table or view.</param>
        /// <param name="whereClause">The where clause. Do not prefix this clause with "WHERE".</param>
        /// <param name="argumentValue">Optional argument value. Every property in the argument value must have a matching parameter in the WHERE clause</param>
        /// <exception cref="ArgumentException">tableOrViewName is empty.;tableOrViewName</exception>
        ITableDbCommandBuilder From(string tableOrViewName, string whereClause, object argumentValue);

        /// <summary>
        /// This is used to directly query a table or view.
        /// </summary>
        /// <param name="tableOrViewName">Name of the table or view.</param>
        /// <param name="filterValue">The filter value is used to generate a simple AND style WHERE clause.</param>
        /// <param name="filterOptions">The filter options.</param>
        /// <returns>ITableDbCommandBuilder.</returns>
        /// <exception cref="ArgumentException">tableOrViewName is empty.;tableOrViewName</exception>
        ITableDbCommandBuilder From(string tableOrViewName, object filterValue, FilterOptions filterOptions = FilterOptions.None);

        /// <summary>
        /// This is used to directly query a table or view.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        ITableDbCommandBuilder From<TObject>() where TObject : class;

        /// <summary>
        /// This is used to directly query a table or view.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="whereClause">The where clause. Do not prefix this clause with "WHERE".</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        ITableDbCommandBuilder From<TObject>(string whereClause) where TObject : class;

        /// <summary>
        /// This is used to directly query a table or view.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="whereClause">The where clause. Do not prefix this clause with "WHERE".</param>
        /// <param name="argumentValue">Optional argument value. Every property in the argument value must have a matching parameter in the WHERE clause</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        ITableDbCommandBuilder From<TObject>(string whereClause, object argumentValue) where TObject : class;

        /// <summary>
        /// This is used to directly query a table or view.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="filterValue">The filter value is used to generate a simple AND style WHERE clause.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        ITableDbCommandBuilder From<TObject>(object filterValue) where TObject : class;

        /// <summary>
        /// Gets a record by its primary key.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>This only works on tables that have a scalar primary key.</remarks>
        ISingleRowDbCommandBuilder GetByKey<TKey>(string tableName, TKey key) where TKey : struct;

        /// <summary>
        /// Gets a record by its primary key.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>This only works on tables that have a scalar primary key.</remarks>
        ISingleRowDbCommandBuilder GetByKey(string tableName, string key);

        /// <summary>
        /// Gets a set of records by their primary key.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        /// <remarks>This only works on tables that have a scalar primary key.</remarks>
        IMultipleRowDbCommandBuilder GetByKeyList<TKey>(string tableName, IEnumerable<TKey> keys);

        /// <summary>Gets a set of records by an unique key.</summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="keyColumn">Name of the key column. This should be a primary or unique key, but that's not enforced.</param>
        /// <param name="keys">The keys.</param>
        /// <returns>IMultipleRowDbCommandBuilder.</returns>
        /// <remarks>This only works on tables that have a scalar primary key.</remarks>
        IMultipleRowDbCommandBuilder GetByKeyList<TKey>(string tableName, string keyColumn, IEnumerable<TKey> keys);

        /// <summary>
        /// Inserts an object into the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The options for how the insert occurs.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">tableName is empty.;tableName</exception>
        IObjectDbCommandBuilder<TArgument> Insert<TArgument>(string tableName, TArgument argumentValue, InsertOptions options = InsertOptions.None) where TArgument : class;

        /// <summary>
        /// Inserts an object into the specified table.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The options for how the insert occurs.</param>
        /// <returns></returns>
        IObjectDbCommandBuilder<TArgument> Insert<TArgument>(TArgument argumentValue, InsertOptions options = InsertOptions.None) where TArgument : class;

        /// <summary>
        /// Update an object in the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The update options.</param>
        /// <exception cref="ArgumentException">tableName is empty.;tableName</exception>
        IObjectDbCommandBuilder<TArgument> Update<TArgument>(string tableName, TArgument argumentValue, UpdateOptions options = UpdateOptions.None) where TArgument : class;

        /// <summary>
        /// Update an object in the specified table.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The update options.</param>
        /// <exception cref="ArgumentException">tableName is empty.;tableName</exception>
        IObjectDbCommandBuilder<TArgument> Update<TArgument>(TArgument argumentValue, UpdateOptions options = UpdateOptions.None) where TArgument : class;

        /// <summary>
        /// Update a record by its primary key.
        /// </summary>
        /// <typeparam name="TArgument">The type of the t argument.</typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="newValues">The new values to use.</param>
        /// <param name="key">The key.</param>
        /// <param name="options">The options.</param>
        /// <returns>MultipleRowDbCommandBuilder&lt;SqlCommand, SqlParameter&gt;.</returns>
        ISingleRowDbCommandBuilder UpdateByKey<TArgument, TKey>(string tableName, TArgument newValues, TKey key, UpdateOptions options = UpdateOptions.None)
            where TKey : struct;

        /// <summary>
        /// Update a record by its primary key.
        /// </summary>
        /// <typeparam name="TArgument">The type of the t argument.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="newValues">The new values to use.</param>
        /// <param name="key">The key.</param>
        /// <param name="options">The options.</param>
        /// <returns>MultipleRowDbCommandBuilder&lt;SqlCommand, SqlParameter&gt;.</returns>
        ISingleRowDbCommandBuilder UpdateByKey<TArgument>(string tableName, TArgument newValues, string key, UpdateOptions options = UpdateOptions.None);

        /// <summary>
        /// Update multiple rows by key.
        /// </summary>
        /// <typeparam name="TArgument">The type of the t argument.</typeparam>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="newValues">The new values to use.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="options">Update options.</param>
        /// <returns>MultipleRowDbCommandBuilder&lt;SqlCommand, SqlParameter&gt;.</returns>
        /// <exception cref="MappingException"></exception>
        IMultipleRowDbCommandBuilder UpdateByKeyList<TArgument, TKey>(string tableName, TArgument newValues, IEnumerable<TKey> keys, UpdateOptions options = UpdateOptions.None);

        /// <summary>
        /// Update multiple records using an update expression.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="updateExpression">The update expression.</param>
        /// <param name="options">The update options.</param>
        /// <remarks>Use .WithFilter to apply a WHERE clause.</remarks>
        IUpdateManyDbCommandBuilder UpdateSet(string tableName, string updateExpression, UpdateOptions options = UpdateOptions.None);

        /// <summary>
        /// Update multiple records using an update expression.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="updateExpression">The update expression.</param>
        /// <param name="updateArgumentValue">The argument for the update expression.</param>
        /// <param name="options">The update options.</param>
        /// <remarks>Use .WithFilter to apply a WHERE clause.</remarks>
        IUpdateManyDbCommandBuilder UpdateSet(string tableName, string updateExpression, object updateArgumentValue, UpdateOptions options = UpdateOptions.None);

        /// <summary>
        /// Update multiple records using an update value.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="newValues">The new values to use.</param>
        /// <param name="options">The options.</param>
        /// <remarks>Use .WithFilter to apply a WHERE clause.</remarks>
        IUpdateManyDbCommandBuilder UpdateSet(string tableName, object newValues, UpdateOptions options = UpdateOptions.None);

        /// <summary>
        /// Perform an insert or update operation as appropriate.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The options for how the insert/update occurs.</param>
        /// <exception cref="ArgumentException">tableName is empty.;tableName</exception>
        IObjectDbCommandBuilder<TArgument> Upsert<TArgument>(string tableName, TArgument argumentValue, UpsertOptions options = UpsertOptions.None) where TArgument : class;

        /// <summary>
        /// Perform an insert or update operation as appropriate.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="options">The options for how the insert/update occurs.</param>
        /// <exception cref="ArgumentException">tableName is empty.;tableName</exception>
        IObjectDbCommandBuilder<TArgument> Upsert<TArgument>(TArgument argumentValue, UpsertOptions options = UpsertOptions.None) where TArgument : class;
    }
}
