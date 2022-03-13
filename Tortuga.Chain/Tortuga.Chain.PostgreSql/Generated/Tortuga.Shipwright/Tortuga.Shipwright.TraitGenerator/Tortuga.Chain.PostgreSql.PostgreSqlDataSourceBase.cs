﻿//This file was generated by Tortuga Shipwright

namespace Tortuga.Chain.PostgreSql
{
	partial class PostgreSqlDataSourceBase: Tortuga.Chain.DataSources.ISupportsDeleteAll, Tortuga.Chain.DataSources.ISupportsTruncate, Tortuga.Chain.DataSources.ISupportsSqlQueries
	{

		private bool __TraitsRegistered;

		// These fields and/or properties hold the traits. They should not be referenced directly.
		private Traits.SupportsDeleteAllTrait<Tortuga.Chain.PostgreSql.PostgreSqlObjectName> ___Trait0 = new();
		private Traits.SupportsDeleteAllTrait<Tortuga.Chain.PostgreSql.PostgreSqlObjectName> __Trait0
		{
			get
			{
				if (!__TraitsRegistered) __RegisterTraits();
				return ___Trait0;
			}
		}
		private Traits.SupportsTruncateTrait<Tortuga.Chain.PostgreSql.PostgreSqlObjectName> ___Trait1 = new();
		private Traits.SupportsTruncateTrait<Tortuga.Chain.PostgreSql.PostgreSqlObjectName> __Trait1
		{
			get
			{
				if (!__TraitsRegistered) __RegisterTraits();
				return ___Trait1;
			}
		}
		private Traits.SupportsSqlQueriesTrait<Npgsql.NpgsqlCommand, Npgsql.NpgsqlParameter> ___Trait2 = new();
		private Traits.SupportsSqlQueriesTrait<Npgsql.NpgsqlCommand, Npgsql.NpgsqlParameter> __Trait2
		{
			get
			{
				if (!__TraitsRegistered) __RegisterTraits();
				return ___Trait2;
			}
		}

		// Explicit interface implementation Tortuga.Chain.DataSources.ISupportsDeleteAll
		Tortuga.Chain.ILink<int?> Tortuga.Chain.DataSources.ISupportsDeleteAll.DeleteAll(System.String tableName)
		{
			return ((Tortuga.Chain.DataSources.ISupportsDeleteAll)__Trait0).DeleteAll(tableName);
		}

		Tortuga.Chain.ILink<int?> Tortuga.Chain.DataSources.ISupportsDeleteAll.DeleteAll<TObject>()
		{
			return ((Tortuga.Chain.DataSources.ISupportsDeleteAll)__Trait0).DeleteAll<TObject>();
		}

		// Explicit interface implementation Tortuga.Chain.DataSources.ISupportsSqlQueries
		Tortuga.Chain.CommandBuilders.IMultipleTableDbCommandBuilder Tortuga.Chain.DataSources.ISupportsSqlQueries.Sql(System.String sqlStatement, System.Object argumentValue)
		{
			return ((Tortuga.Chain.DataSources.ISupportsSqlQueries)__Trait2).Sql(sqlStatement, argumentValue);
		}

		// Explicit interface implementation Tortuga.Chain.DataSources.ISupportsTruncate
		Tortuga.Chain.ILink<int?> Tortuga.Chain.DataSources.ISupportsTruncate.Truncate(System.String tableName)
		{
			return ((Tortuga.Chain.DataSources.ISupportsTruncate)__Trait1).Truncate(tableName);
		}

		Tortuga.Chain.ILink<int?> Tortuga.Chain.DataSources.ISupportsTruncate.Truncate<TObject>()
		{
			return ((Tortuga.Chain.DataSources.ISupportsTruncate)__Trait1).Truncate<TObject>();
		}

		// Exposing trait Traits.SupportsDeleteAllTrait<Tortuga.Chain.PostgreSql.PostgreSqlObjectName>

		/// <summary>Deletes all records in the specified table.</summary>
		/// <param name="tableName">Name of the table to clear.</param>
		/// <returns>The number of rows deleted or null if the database doesn't provide that information.</returns>
		public Tortuga.Chain.ILink<int?> DeleteAll(Tortuga.Chain.PostgreSql.PostgreSqlObjectName tableName)
		{
			return __Trait0.DeleteAll(tableName);
		}

		/// <summary>Deletes all records in the specified table.</summary>
		/// <typeparam name="TObject">This class used to determine which table to clear</typeparam>
		/// <returns>The number of rows deleted or null if the database doesn't provide that information.</returns>
		public Tortuga.Chain.ILink<int?> DeleteAll<TObject>()where TObject : class
		{
			return __Trait0.DeleteAll<TObject>();
		}

		// Exposing trait Traits.SupportsSqlQueriesTrait<Npgsql.NpgsqlCommand, Npgsql.NpgsqlParameter>

		/// <summary>
		/// Creates a operation based on a raw SQL statement.
		/// </summary>
		/// <param name="sqlStatement">The SQL statement.</param>
		/// <returns></returns>
		public Tortuga.Chain.CommandBuilders.MultipleTableDbCommandBuilder<Npgsql.NpgsqlCommand, Npgsql.NpgsqlParameter> Sql(System.String sqlStatement)
		{
			return __Trait2.Sql(sqlStatement);
		}

		/// <summary>
		/// Creates a operation based on a raw SQL statement.
		/// </summary>
		/// <param name="sqlStatement">The SQL statement.</param>
		/// <param name="argumentValue">The argument value.</param>
		/// <returns>SqlServerSqlCall.</returns>
		public Tortuga.Chain.CommandBuilders.MultipleTableDbCommandBuilder<Npgsql.NpgsqlCommand, Npgsql.NpgsqlParameter> Sql(System.String sqlStatement, System.Object argumentValue)
		{
			return __Trait2.Sql(sqlStatement, argumentValue);
		}

		// Exposing trait Traits.SupportsTruncateTrait<Tortuga.Chain.PostgreSql.PostgreSqlObjectName>

		/// <summary>Truncates the specified table.</summary>
		/// <param name="tableName">Name of the table to Truncate.</param>
		/// <returns>The number of rows deleted or null if the database doesn't provide that information.</returns>
		public Tortuga.Chain.ILink<int?> Truncate(Tortuga.Chain.PostgreSql.PostgreSqlObjectName tableName)
		{
			return __Trait1.Truncate(tableName);
		}

		/// <summary>Truncates the specified table.</summary>
		/// <typeparam name="TObject">This class used to determine which table to Truncate</typeparam>
		/// <returns>The number of rows deleted or null if the database doesn't provide that information.</returns>
		public Tortuga.Chain.ILink<int?> Truncate<TObject>()where TObject : class
		{
			return __Trait1.Truncate<TObject>();
		}

		private partial Tortuga.Chain.ILink<int?> OnDeleteAll(Tortuga.Chain.PostgreSql.PostgreSqlObjectName tableName );

		private partial Tortuga.Chain.PostgreSql.PostgreSqlObjectName OnGetTableOrViewNameFromClass(System.Type type, Tortuga.Chain.Metadata.OperationType operationType );

		// Reusing the previously declared partial method named OnGetTableOrViewNameFromClass declared on trait Traits.SupportsDeleteAllTrait<Tortuga.Chain.PostgreSql.PostgreSqlObjectName>

		private partial Tortuga.Chain.PostgreSql.PostgreSqlObjectName OnParseObjectName(System.String objectName );

		// Reusing the previously declared partial method named OnParseObjectName declared on trait Traits.SupportsDeleteAllTrait<Tortuga.Chain.PostgreSql.PostgreSqlObjectName>

		private partial Tortuga.Chain.CommandBuilders.MultipleTableDbCommandBuilder<Npgsql.NpgsqlCommand, Npgsql.NpgsqlParameter> OnSql(System.String sqlStatement, System.Object? argumentValue );

		private partial Tortuga.Chain.ILink<int?> OnTruncate(Tortuga.Chain.PostgreSql.PostgreSqlObjectName tableName );


		private void __RegisterTraits()
		{
			__TraitsRegistered = true;
			__Trait0.OnGetTableOrViewNameFromClass = OnGetTableOrViewNameFromClass;
			__Trait0.OnDeleteAll = OnDeleteAll;
			__Trait0.OnParseObjectName = OnParseObjectName;
			__Trait1.OnGetTableOrViewNameFromClass = OnGetTableOrViewNameFromClass;
			__Trait1.OnParseObjectName = OnParseObjectName;
			__Trait1.OnTruncate = OnTruncate;
			__Trait2.OnSql = OnSql;
		}
	}
}
