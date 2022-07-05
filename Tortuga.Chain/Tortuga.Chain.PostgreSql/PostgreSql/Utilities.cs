﻿using Npgsql;
using NpgsqlTypes;
using Tortuga.Chain.CommandBuilders;

namespace Tortuga.Chain.PostgreSql;

internal static class Utilities
{
	/// <summary>
	/// Gets the parameters from a SQL Builder.
	/// </summary>
	/// <param name="sqlBuilder">The SQL builder.</param>
	/// <returns></returns>
	public static List<NpgsqlParameter> GetParameters(this SqlBuilder<NpgsqlDbType> sqlBuilder)
	{
		return sqlBuilder.GetParameters(ParameterBuilderCallback);
	}

	public static NpgsqlParameter ParameterBuilderCallback(SqlBuilderEntry<NpgsqlDbType> entry)
	{
		var result = new NpgsqlParameter();
		result.ParameterName = entry.Details.SqlVariableName;
		result.Value = entry.ParameterValue;
		if (entry.Details.DbType.HasValue)
			result.NpgsqlDbType = entry.Details.DbType.Value;
		return result;
	}

		public static bool PrimaryKeyIsIdentity(this SqlBuilder<NpgsqlDbType> sqlBuilder, out List<NpgsqlParameter> keyParameters)
		{
			return sqlBuilder.PrimaryKeyIsIdentity((NpgsqlDbType? type) =>
			{
				var result = new NpgsqlParameter();
				if (type.HasValue)
					result.NpgsqlDbType = type.Value;
				return result;
			}, out keyParameters);
		}

		public static bool RequiresSorting(this PostgreSqlLimitOption limitOption)
		{
			return limitOption switch
			{
				PostgreSqlLimitOption.None => false,
				PostgreSqlLimitOption.Rows => true,
				PostgreSqlLimitOption.TableSampleSystemPercentage => false,
				PostgreSqlLimitOption.TableSampleBernoulliPercentage => false,
				_ => throw new ArgumentOutOfRangeException(nameof(limitOption), limitOption, "Unknown limit option")
			};
		}
	}
}