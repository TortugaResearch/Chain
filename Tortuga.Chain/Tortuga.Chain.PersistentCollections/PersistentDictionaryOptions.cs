namespace Tortuga.Chain.PersistentCollections;

/// <summary>
/// PersistentDictionaryOptions is used for controlling how the dictionary's backing table will be created.
/// </summary>
public class PersistentDictionaryOptions
{
	/// <summary>
	/// Gets or sets the name of the key column. This defaults to "KeyColumn".
	/// </summary>
	/// <value>The name of the key column.</value>
	public string KeyColumnName { get; set; } = "KeyColumn";

	/// <summary>
	/// Gets or sets the maximum length of the key.
	/// </summary>
	/// <value>The maximum length of the key.</value>
	/// <remarks>This only applies to string columns.</remarks>
	public int? KeyMaxLength { get; set; }

	/// <summary>
	/// Gets or sets the key's precision.
	/// </summary>
	/// <value>The key precision.</value>
	/// <remarks>This only applies to decimal columns.</remarks>
	public int? KeyPrecision { get; set; }

	/// <summary>
	/// Gets or sets the key's scale.
	/// </summary>
	/// <value>The key scale.</value>
	/// <remarks>This only applies to decimal columns.</remarks>
	public int? KeyScale { get; set; }

	/// <summary>
	/// Gets or sets the name of the value column. This defaults to "ValueColumn".
	/// </summary>
	/// <value>The name of the value column.</value>
	public string ValueColumnName { get; set; } = "ValueColumn";

	/// <summary>
	/// Gets or sets a value indicating whether the value is nullable.
	/// </summary>
	/// <remarks>This only applies to string columns.</remarks>
	public bool? ValueIsNullable { get; set; }

	/// <summary>
	/// Gets or sets the maximum length of the value.
	/// </summary>
	/// <value>The maximum length of the value.</value>
	/// <remarks>This only applies to string columns.</remarks>
	public int? ValueMaxLength { get; set; }

	/// <summary>
	/// Gets or sets the value's precision.
	/// </summary>
	/// <value>The value precision.</value>
	/// <remarks>This only applies to decimal columns.</remarks>
	public int? ValuePrecision { get; set; }

	/// <summary>
	/// Gets or sets the value's scale.
	/// </summary>
	/// <value>The value scale.</value>
	/// <remarks>This only applies to decimal columns.</remarks>
	public int? ValueScale { get; set; }
}
