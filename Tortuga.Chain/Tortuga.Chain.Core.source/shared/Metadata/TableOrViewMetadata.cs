using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Tortuga.Chain.Metadata
{
    /// <summary>
    /// Abstract version of TableOrViewMetadata.
    /// </summary>
    public abstract class TableOrViewMetadata
    {
        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ColumnMetadataCollection Columns { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this table or view has primary key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a primary key; otherwise, <c>false</c>.
        /// </value>
        public bool HasPrimaryKey => Columns.Any(c => c.IsPrimaryKey);

        /// <summary>
        /// Gets a value indicating whether this instance is table or a view.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is a table; otherwise, <c>false</c>.
        /// </value>
        public bool IsTable { get; protected set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the columns known to be not nullable.
        /// </summary>
        /// <value>
        /// The nullable columns.
        /// </value>
        /// <remarks>This is used to improve the performance of materializers by avoiding is null checks.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ColumnMetadataCollection NonNullableColumns { get; protected set; }
    }
}
