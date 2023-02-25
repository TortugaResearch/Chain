using Tortuga.Chain.DataSources;

namespace Tortuga.Chain.PersistentCollections;

/// <summary>
/// Extension methods for Persistent Collection classes.
/// </summary>
public static class PersistentCollections
{
	/// <summary>
	/// Builer for a PersistentDictionary.
	/// </summary>
	/// <typeparam name="TDataSource">The type of the data source.</typeparam>
	/// <param name="dataSource">The data source.</param>
	/// <returns>PersistentDictionary builder.</returns>
	public static PersistentDictionary<TDataSource> PersistentDictionary<TDataSource>(this TDataSource dataSource)
		where TDataSource : IRootDataSource, ICrudDataSource
	{
		return new PersistentDictionary<TDataSource>(dataSource);
	}
}
