﻿//This file was generated by Tortuga Shipwright

namespace Tortuga.Chain.PostgreSql
{
	partial class PostgreSqlOpenDataSource: Tortuga.Chain.DataSources.IOpenDataSource
	{

		private bool __TraitsRegistered;

		// These fields and/or properties hold the traits. They should not be referenced directly.
		private Traits.OpenDataSourceTrait<Tortuga.Chain.PostgreSqlDataSource, Tortuga.Chain.PostgreSql.PostgreSqlOpenDataSource, Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction, Npgsql.NpgsqlCommand, Tortuga.Chain.PostgreSql.PostgreSqlMetadataCache> ___Trait0 = new();
		private Traits.OpenDataSourceTrait<Tortuga.Chain.PostgreSqlDataSource, Tortuga.Chain.PostgreSql.PostgreSqlOpenDataSource, Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction, Npgsql.NpgsqlCommand, Tortuga.Chain.PostgreSql.PostgreSqlMetadataCache> __Trait0
		{
			get
			{
				if (!__TraitsRegistered) __RegisterTraits();
				return ___Trait0;
			}
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

		System.Boolean Tortuga.Chain.DataSources.IOpenDataSource.TryRollback()
		{
			return ((Tortuga.Chain.DataSources.IOpenDataSource)__Trait0).TryRollback();
		}

		// Exposing trait Traits.OpenDataSourceTrait<Tortuga.Chain.PostgreSqlDataSource, Tortuga.Chain.PostgreSql.PostgreSqlOpenDataSource, Npgsql.NpgsqlConnection, Npgsql.NpgsqlTransaction, Npgsql.NpgsqlCommand, Tortuga.Chain.PostgreSql.PostgreSqlMetadataCache>

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
		public   Npgsql.NpgsqlTransaction? AssociatedTransaction
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
		/// Closes the connection and transaction associated with this data source.
		/// </summary>
		public void Close()
		{
			__Trait0.Close();
		}

		/// <summary>
		/// Gets the database metadata.
		/// </summary>
		public  override  Tortuga.Chain.PostgreSql.PostgreSqlMetadataCache DatabaseMetadata
		{
			get => __Trait0.DatabaseMetadata;
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
		
		private   Npgsql.NpgsqlTransaction? m_Transaction
		{
			get => __Trait0.m_Transaction;
			init => __Trait0.m_Transaction = value;
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

		/// <summary>
		/// Tries the commit the transaction associated with this data source.
		/// </summary>
		/// <returns>True if there was an open transaction associated with this data source, otherwise false.</returns>
		public System.Boolean TryCommit()
		{
			return __Trait0.TryCommit();
		}

		/// <summary>
		/// Tries to commits the transaction and disposes the underlying connection.
		/// </summary>
		/// <returns>True if there was an open transaction associated with this data source, otherwise false.</returns>
		public System.Threading.Tasks.Task<bool> TryCommitAsync(System.Threading.CancellationToken cancellationToken = default)
		{
			return __Trait0.TryCommitAsync(cancellationToken);
		}

		/// <summary>
		/// Tries to rollback the transaction associated with this data source.
		/// </summary>
		/// <returns>True if there was an open transaction associated with this data source, otherwise false.</returns>
		public System.Boolean TryRollback()
		{
			return __Trait0.TryRollback();
		}

		/// <summary>
		/// Tries to roll back the transaction to the indicated save point.
		/// </summary>
		/// <param name="savepointName">The name of the savepoint to roll back to.</param>
		/// <returns>True if there was an open transaction associated with this data source, otherwise false.</returns>
		public System.Boolean TryRollback(System.String savepointName)
		{
			return __Trait0.TryRollback(savepointName);
		}

		/// <summary>
		/// Tries to roll back the transaction.
		/// </summary>
		/// <returns>True if there was an open transaction associated with this data source, otherwise false.</returns>
		public System.Threading.Tasks.Task<bool> TryRollbackAsync(System.Threading.CancellationToken cancellationToken = default)
		{
			return __Trait0.TryRollbackAsync(cancellationToken);
		}

		/// <summary>
		/// Tries to roll back the transaction to the indicated save point.
		/// </summary>
		/// <param name="savepointName">The name of the savepoint to roll back to.</param>
		/// <param name="cancellationToken"></param>
		/// <returns>True if there was an open transaction associated with this data source, otherwise false.</returns>
		public System.Threading.Tasks.Task<bool> TryRollbackAsync(System.String savepointName, System.Threading.CancellationToken cancellationToken = default)
		{
			return __Trait0.TryRollbackAsync(savepointName, cancellationToken);
		}

		/// <summary>
		/// Tries to create a savepoint in the transaction. This allows all commands that are executed after the savepoint was established to be rolled back, restoring the transaction state to what it was at the time of the savepoint.
		/// </summary>
		/// <param name="savepointName">The name of the savepoint to be created.</param>
		/// <returns>True if there was an open transaction associated with this data source, otherwise false.</returns>
		public System.Boolean TrySave(System.String savepointName)
		{
			return __Trait0.TrySave(savepointName);
		}

		/// <summary>
		/// Tries to creates a savepoint in the transaction. This allows all commands that are executed after the savepoint was established to be rolled back, restoring the transaction state to what it was at the time of the savepoint.
		/// </summary>
		/// <param name="savepointName">The name of the savepoint to be created.</param>
		/// <param name="cancellationToken"></param>
		/// <returns>True if there was an open transaction associated with this data source, otherwise false.</returns>
		public System.Threading.Tasks.Task<bool> TrySaveAsync(System.String savepointName, System.Threading.CancellationToken cancellationToken = default)
		{
			return __Trait0.TrySaveAsync(savepointName, cancellationToken);
		}

		/// <summary>
		/// Modifies this data source with additional audit rules.
		/// </summary>
		/// <param name="additionalRules">The additional rules.</param>
		/// <returns></returns>
		public Tortuga.Chain.PostgreSql.PostgreSqlOpenDataSource WithRules(params Tortuga.Chain.AuditRules.AuditRule[] additionalRules)
		{
			return __Trait0.WithRules(additionalRules);
		}

		/// <summary>
		/// Modifies this data source with additional audit rules.
		/// </summary>
		/// <param name="additionalRules">The additional rules.</param>
		/// <returns></returns>
		public Tortuga.Chain.PostgreSql.PostgreSqlOpenDataSource WithRules(System.Collections.Generic.IEnumerable<Tortuga.Chain.AuditRules.AuditRule> additionalRules)
		{
			return __Trait0.WithRules(additionalRules);
		}

		/// <summary>
		/// Modifies this data source to include the indicated user.
		/// </summary>
		/// <param name="userValue">The user value.</param>
		/// <returns></returns>
		/// <remarks>
		/// This is used in conjunction with audit rules.
		/// </remarks>
		public Tortuga.Chain.PostgreSql.PostgreSqlOpenDataSource WithUser(System.Object? userValue)
		{
			return __Trait0.WithUser(userValue);
		}

		private partial Tortuga.Chain.PostgreSql.PostgreSqlOpenDataSource OnOverride(System.Collections.Generic.IEnumerable<Tortuga.Chain.AuditRules.AuditRule>? additionalRules, System.Object? userValue );

		private void __RegisterTraits()
		{
			__TraitsRegistered = true;
			__Trait0.OnOverride = OnOverride;
			__Trait0.Container = this;
			__Trait0.DisposableContainer = this as Traits.IHasOnDispose;
		}

	}
}
