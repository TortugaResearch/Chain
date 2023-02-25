using System.Xml.Linq;

namespace Tortuga.Chain.CommandBuilders;

/// <summary>
/// This allows the use of scalar materializers against a command builder.
/// </summary>
/// <remarks>Warning: This interface is meant to simulate multiple inheritance and work-around some issues with exposing generic types. Do not implement it in client code, as new methods will be added over time.</remarks>
public interface IScalarDbCommandBuilder : IDbCommandBuilder
{
	/// <summary>
	/// Indicates the results should be materialized as a Boolean.
	/// </summary>
	ILink<bool> ToBoolean();

	/// <summary>
	/// Indicates the results should be materialized as a Boolean.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<bool> ToBoolean(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable Boolean.
	/// </summary>
	ILink<bool?> ToBooleanOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable Boolean.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<bool?> ToBooleanOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a byte.
	/// </summary>
	ILink<byte> ToByte();

	/// <summary>
	/// Indicates the results should be materialized as a byte.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<byte> ToByte(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a byte array.
	/// </summary>
	ILink<byte[]> ToByteArray();

	/// <summary>
	/// Indicates the results should be materialized as a byte array.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<byte[]> ToByteArray(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable byte.
	/// </summary>
	ILink<byte?> ToByteOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable byte.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<byte?> ToByteOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a non-nullable char.
	/// </summary>
	/// <returns></returns>
	ILink<char> ToChar();

	/// <summary>
	/// Indicates the results should be materialized as a non-nullable char.
	/// </summary>
	/// <returns></returns>
	ILink<char> ToChar(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable char.
	/// </summary>
	/// <returns></returns>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<char?> ToCharOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable char.
	/// </summary>
	/// <returns></returns>
	ILink<char?> ToCharOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a DateTime.
	/// </summary>
	ILink<DateTime> ToDateTime();

	/// <summary>
	/// Indicates the results should be materialized as a DateTime.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<DateTime> ToDateTime(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a DateTimeOffset.
	/// </summary>
	ILink<DateTimeOffset> ToDateTimeOffset();

	/// <summary>
	/// Indicates the results should be materialized as a DateTimeOffset.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<DateTimeOffset> ToDateTimeOffset(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable DateTimeOffset.
	/// </summary>
	ILink<DateTimeOffset?> ToDateTimeOffsetOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable DateTimeOffset.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<DateTimeOffset?> ToDateTimeOffsetOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable DateTime.
	/// </summary>
	ILink<DateTime?> ToDateTimeOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable DateTime.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<DateTime?> ToDateTimeOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a Decimal.
	/// </summary>
	ILink<decimal> ToDecimal();

	/// <summary>
	/// Indicates the results should be materialized as a Decimal.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<decimal> ToDecimal(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable Decimal.
	/// </summary>
	ILink<decimal?> ToDecimalOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable Decimal.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<decimal?> ToDecimalOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a Double.
	/// </summary>
	ILink<double> ToDouble();

	/// <summary>
	/// Indicates the results should be materialized as a Double.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<double> ToDouble(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable Double.
	/// </summary>
	ILink<double?> ToDoubleOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable Double.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<double?> ToDoubleOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a Guid.
	/// </summary>
	ILink<Guid> ToGuid();

	/// <summary>
	/// Indicates the results should be materialized as a Guid.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<Guid> ToGuid(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable Guid.
	/// </summary>
	ILink<Guid?> ToGuidOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable Guid.
	/// </summary>
	ILink<Guid?> ToGuidOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a Int16.
	/// </summary>
	ILink<short> ToInt16();

	/// <summary>
	/// Indicates the results should be materialized as a Int16.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<short> ToInt16(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable Int16.
	/// </summary>
	ILink<short?> ToInt16OrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable Int16.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<short?> ToInt16OrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a Int32.
	/// </summary>
	ILink<int> ToInt32();

	/// <summary>
	/// Indicates the results should be materialized as a Int32.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<int> ToInt32(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable Int32.
	/// </summary>
	ILink<int?> ToInt32OrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable Int32.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<int?> ToInt32OrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a Int64.
	/// </summary>
	ILink<long> ToInt64();

	/// <summary>
	/// Indicates the results should be materialized as a Int64.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<long> ToInt64(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable Int64.
	/// </summary>
	ILink<long?> ToInt64OrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable Int64.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<long?> ToInt64OrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a Single.
	/// </summary>
	ILink<float> ToSingle();

	/// <summary>
	/// Indicates the results should be materialized as a Single.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<float> ToSingle(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable Single.
	/// </summary>
	ILink<float?> ToSingleOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable Single.
	/// </summary>
	ILink<float?> ToSingleOrNull(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable string.
	/// </summary>
	/// <returns></returns>
	ILink<string> ToString();

	/// <summary>
	/// Indicates the results should be materialized as a nullable string.
	/// </summary>
	/// <returns></returns>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<string> ToString(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a TimeSpan.
	/// </summary>
	ILink<TimeSpan> ToTimeSpan();

	/// <summary>
	/// Indicates the results should be materialized as a TimeSpan.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<TimeSpan> ToTimeSpan(string columnName);

	/// <summary>
	/// Indicates the results should be materialized as a nullable TimeSpan.
	/// </summary>
	ILink<TimeSpan?> ToTimeSpanOrNull();

	/// <summary>
	/// Indicates the results should be materialized as a nullable TimeSpan.
	/// </summary>
	/// <param name="columnName">Name of the desired column.</param>
	ILink<TimeSpan?> ToTimeSpanOrNull(string columnName);

	/// <summary>
	/// Materializes the result as an XElement.
	/// </summary>
	/// <returns>ILink&lt;XElement&gt;.</returns>
	ILink<XElement> ToXml();

	/// <summary>
	/// Materializes the result as an XElement.
	/// </summary>
	/// <param name="columnName">Name of the column.</param>
	/// <returns>ILink&lt;XElement&gt;.</returns>
	ILink<XElement> ToXml(string columnName);
}
