namespace BDSP.Core.Berries
{
    /// <summary>
    /// Lightweight, type-safe wrapper for a berry's unique identifier. The value is an index into <see cref="BerryTable"/>.
    /// This struct prevents accidental mixing of different types of IDs and improves code clarity.
    /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:BerryIdTable"]/*' />
    /// </summary>
    public readonly struct BerryId
    {
        /// <summary>
        /// The underlying numeric value of the berry ID.
        /// This value corresponds to the index in the <see cref="BerryTable"/>.
        /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:BerryIdTable"]/*' />
        /// </summary>
        public readonly ushort Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BerryId"/> struct.
        /// </summary>
        /// <param name="value">The numeric value of the ID.</param>
        public BerryId(ushort value) => Value = value;

        public override string ToString() => Value.ToString();
    }
}
