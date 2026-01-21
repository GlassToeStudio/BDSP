using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDSP.Core.Berries
{
    /// <summary>
    /// A lightweight, type-safe wrapper for a berry's unique identifier.
    /// This struct prevents accidental mixing of different types of IDs and improves code clarity.
    /// </summary>
    public readonly struct BerryId
    {
        /// <summary>
        /// The underlying numeric value of the berry ID.
        /// This value corresponds to the index in the <see cref="BerryTable"/>.
        /// </summary>
        public readonly ushort Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BerryId"/> struct.
        /// </summary>
        /// <param name="value">The numeric value of the ID.</param>
        public BerryId(ushort value) => Value = value;
    }
}
