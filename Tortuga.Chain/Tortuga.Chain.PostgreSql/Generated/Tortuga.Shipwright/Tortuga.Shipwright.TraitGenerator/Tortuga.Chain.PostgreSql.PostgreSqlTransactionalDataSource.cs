﻿//This file was generated by Tortuga Shipwright

namespace Tortuga.Chain.PostgreSql
{
	partial class PostgreSqlTransactionalDataSource: Tortuga.Chain.DataSources.ITransactionalDataSource, Tortuga.Chain.DataSources.IOpenDataSource, System.IDisposable
	{

		private bool __TraitsRegistered;

		// These fields and/or properties hold the traits. They should not be referenced directly.
		private Traits.TransactionDataSourceTrait<Tortuga.Chain.PostgreSqlDataSource, Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction, Npgsql.NpgsqlCommand> ___Trait0 = new();
		private Traits.TransactionDataSourceTrait<Tortuga.Chain.PostgreSqlDataSource, Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction, Npgsql.NpgsqlCommand> __Trait0
		{
			get
			{
				if (!__TraitsRegistered) __RegisterTraits();
				return ___Trait0;
			}
		}

		// Explicit interface implementation System.IDisposable
		void System.IDisposable.Dispose()
		{
			((System.IDisposable)__Trait0).Dispose();
		}

		// Explicit interface implementation Tortuga.Chain.DataSources.IOpenDataSource
		System.Data.Common.DbConnection Tortuga.Chain.DataSources.IOpenDataSource.AssociatedConnection
		{
			get => ((Tortuga.Chain.DataSources.IOpenDataSource)__Trait0).AssociatedConnection;
		}
		System.Data.Common.DbTransaction? Tortuga.Chain.DataSources.IOpenDataSource.AssociatedTransaction
		{
			get => ((Tortuga.Chain.DataSources.IOpenDataSource)__Trait0).AssociatedTransaction;
		}
		void Tortuga.Chain.DataSources.IOpenDataSource.Close()
		{
			((Tortuga.Chain.DataSources.IOpenDataSource)__Trait0).Close();
		}

		System.Boolean Tortuga.Chain.DataSources.IOpenDataSource.TryCommit()
		{
			return ((Tortuga.Chain.DataSources.IOpenDataSource)__Trait0).TryCommit();
		}

		// Explicit interface implementation Tortuga.Chain.DataSources.ITransactionalDataSource
		void Tortuga.Chain.DataSources.ITransactionalDataSource.Commit()
		{
			((Tortuga.Chain.DataSources.ITransactionalDataSource)__Trait0).Commit();
		}

		// Exposing trait Traits.TransactionDataSourceTrait<Tortuga.Chain.PostgreSqlDataSource, Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction, Npgsql.NpgsqlCommand>

		/// <summary>
		/// Returns the associated connection.
		/// </summary>
		/// <value>The associated connection.</value>
		public   Npgsql.NpgsqlConnection AssociatedConnection
		{
			get => __Trait0.AssociatedConnection;
		}
		/// <summary>
		/// Returns the associated transaction.
		/// </summary>
		/// <value>The associated transaction.</value>
		public   Npgsql.NpgsqlTransaction AssociatedTransaction
		{
			get => __Trait0.AssociatedTransaction;
		}
		/// <summary>
		/// Gets or sets the cache to be used by this data source. The default is .NET's System.Runtime.Caching.MemoryCache.
		/// </summary>
		public  override  Tortuga.Chain.Core.ICacheAdapter Cache
		{
			get => __Trait0.Cache;
		}
		/// <summary>
		/// Commits the transaction and disposes the underlying connection.
		/// </summary>
		public void Commit()
		{
			__Trait0.Commit();
		}

		/// <summary>
		/// Closes the current transaction and connection. If not committed, the transaction is rolled back.
		/// </summary>
		public void Dispose()
		{
			__Trait0.Dispose();
		}

		/// <summary>
		/// Closes the current transaction and connection. If not committed, the transaction is rolled back.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(System.Boolean disposing)
		{
			__Trait0.Dispose(disposing);
		}

		/// <summary>
		/// The extension cache is used by extensions to store data source specific information.
		/// </summary>
		/// <value>
		/// The extension cache.
		/// </value>
		protected  override  System.Collections.Concurrent.ConcurrentDictionary<System.Type, object> ExtensionCache
		{
			get => __Trait0.ExtensionCache;
		}
		
		private   Tortuga.Chain.PostgreSqlDataSource m_BaseDataSource
		{
			get => __Trait0.m_BaseDataSource;
			init => __Trait0.m_BaseDataSource = value;
		}
		
		private   Npgsql.NpgsqlConnection m_Connection
		{
			get => __Trait0.m_Connection;
			init => __Trait0.m_Connection = value;
		}
		
		private   System.Boolean m_Disposed
		{
			get => __Trait0.m_Disposed;
			set => __Trait0.m_Disposed = value;
		}
		
		private   Npgsql.NpgsqlTransaction m_Transaction
		{
			get => __Trait0.m_Transaction;
			init => __Trait0.m_Transaction = value;
		}
		/// <summary>
		/// Rolls back the transaction and disposes the underlying connection.
		/// </summary>
		public void Rollback()
		{
			__Trait0.Rollback();
		}

		/// <summary>
		/// Tests the connection.
		/// </summary>
		public override void TestConnection()
		{
			__Trait0.TestConnection();
		}

		/// <summary>
		/// Tests the connection asynchronously.
		/// </summary>
		/// <returns></returns>
		public override System.Threading.Tasks.Task TestConnectionAsync()
		{
			return __Trait0.TestConnectionAsync();
		}

		private partial void AdditionalDispose( );

		private void __RegisterTraits()
		{
			__TraitsRegistered = true;
			__Trait0.AdditionalDispose = AdditionalDispose;
			__Trait0.Container = this;
		}

	}
}
