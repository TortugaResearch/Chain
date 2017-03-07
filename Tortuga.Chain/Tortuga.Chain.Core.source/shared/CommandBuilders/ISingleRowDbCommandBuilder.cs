using System;
using System.Xml.Linq;

#if !DataTable_Missing
using System.Data;
#endif

namespace Tortuga.Chain.CommandBuilders
{

    /// <summary>
    /// This allows the use of scalar and single row materializers against a command builder.
    /// </summary>
    /// <remarks>Warning: This interface is meant to simulate multiple inheritance and work-around some issues with exposing generic types. Do not implement it in client code, as new methods will be added over time.</remarks>
    public interface ISingleRowDbCommandBuilder : IScalarDbCommandBuilder
    {

        /// <summary>
        /// Materializes the result as an instance of the indicated type
        /// </summary>
        /// <typeparam name="TObject">The type of the object returned.</typeparam>
        /// <param name="rowOptions">The row options.</param>
        /// <returns></returns>
        IConstructibleMaterializer<TObject> ToObject<TObject>(RowOptions rowOptions = RowOptions.None)
            where TObject : class;

        /// <summary>
        /// Materializes the result as a dynamic object
        /// </summary>
        /// <param name="rowOptions">The row options.</param>
        /// <returns></returns>
        ILink<dynamic> ToDynamicObject(RowOptions rowOptions = RowOptions.None);

        /// <summary>
        /// Indicates the results should be materialized as a Row.
        /// </summary>
        ILink<Row> ToRow(RowOptions rowOptions = RowOptions.None);


#if !DataTable_Missing
        /// <summary>
        /// Indicates the results should be materialized as a Row.
        /// </summary>
        ILink<DataRow> ToDataRow(RowOptions rowOptions = RowOptions.None);
#endif




    }
}
