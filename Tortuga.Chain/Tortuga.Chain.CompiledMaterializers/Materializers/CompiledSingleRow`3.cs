﻿using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Tortuga.Chain.CommandBuilders;

namespace Tortuga.Chain.Materializers
{
	/// <summary>
	/// Extension for using compiled materializers with Tortuga Chain
	/// </summary>
	/// <typeparam name="TCommand">The type of the command.</typeparam>
	/// <typeparam name="TParameter">The type of the parameter.</typeparam>
	/// <typeparam name="TObject">The type of the result object.</typeparam>
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct CompiledSingleRow<TCommand, TParameter, TObject>
			where TCommand : DbCommand
			where TParameter : DbParameter
			where TObject : class, new()
	{
		readonly SingleRowDbCommandBuilder<TCommand, TParameter> m_CommandBuilder;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMultipleRow{TCommand, TParameter}"/> struct.
		/// </summary>
		/// <param name="commandBuilder">The command builder.</param>
		public CompiledSingleRow(SingleRowDbCommandBuilder<TCommand, TParameter> commandBuilder)
		{
			m_CommandBuilder = commandBuilder;
		}

		/// <summary>
		/// Materializes the result as an instance of the indicated type
		/// </summary>
		/// <param name="rowOptions">The row options.</param>
		/// <returns></returns>
		public ILink<TObject> ToObject(RowOptions rowOptions = RowOptions.None)
		{
			return new CompiledObjectMaterializer<TCommand, TParameter, TObject>(m_CommandBuilder, rowOptions);
		}

		/// <summary>
		/// Materializes the result as an instance of the indicated type
		/// </summary>
		/// <param name="rowOptions">The row options.</param>
		/// <returns></returns>
		public ILink<TObject?> ToObjectOrNull(RowOptions rowOptions = RowOptions.None)
		{
			return new CompiledObjectOrNullMaterializer<TCommand, TParameter, TObject>(m_CommandBuilder, rowOptions);
		}

		/// <summary>
		/// Materializes the result as an instance of the indicated type
		/// </summary>
		/// <typeparam name="TResultObject">The type of the object returned.</typeparam>
		/// <param name="rowOptions">The row options.</param>
		/// <returns></returns>
		public ILink<TResultObject> ToObject<TResultObject>(RowOptions rowOptions = RowOptions.None)
			where TResultObject : class, new()
		{
			return new CompiledObjectMaterializer<TCommand, TParameter, TResultObject>(m_CommandBuilder, rowOptions);
		}

		/// <summary>
		/// Materializes the result as an instance of the indicated type
		/// </summary>
		/// <typeparam name="TResultObject">The type of the object returned.</typeparam>
		/// <param name="rowOptions">The row options.</param>
		/// <returns></returns>
		public ILink<TResultObject?> ToObjectOrNull<TResultObject>(RowOptions rowOptions = RowOptions.None)
			where TResultObject : class, new()
		{
			return new CompiledObjectOrNullMaterializer<TCommand, TParameter, TResultObject>(m_CommandBuilder, rowOptions);
		}
	}
}
