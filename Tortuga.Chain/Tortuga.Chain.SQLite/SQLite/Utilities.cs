﻿using System.Data.SQLite;
using Tortuga.Chain.CommandBuilders;

namespace Tortuga.Chain.SQLite
{
	internal static class Utilities
	{
		/// <summary>
		/// Gets the parameters from a SQL Builder.
		/// </summary>
		/// <param name="sqlBuilder">The SQL builder.</param>
		/// <returns></returns>
		public static List<SQLiteParameter> GetParameters(this SqlBuilder<DbType> sqlBuilder)
		{
			return sqlBuilder.GetParameters(ParameterBuilderCallback);
		}

		public static SQLiteParameter ParameterBuilderCallback(SqlBuilderEntry<DbType> entry)
		{
			var result = new SQLiteParameter();
			result.ParameterName = entry.Details.SqlVariableName;
			result.Value = entry.ParameterValue;

			if (entry.Details.DbType.HasValue)
				result.DbType = entry.Details.DbType.Value;

			return result;
		}
	}
}
