namespace BDSP.Core.Berries
{
    /// <summary>
    /// Flavor categories used by berries and poffins.
    /// </summary>
    /// <remarks>
    /// Flavor X is weakened by Flavor Y:
    /// <br/>X -> Y <br/>
    /// <include file='BerryDocs.xml' path='doc/members/member[@name="T:FlavorPreference"]/*' />
    /// </remarks>
    public enum Flavor : byte
    {
        /// <summary>Spicy flavor.</summary>
        Spicy = 0,
        /// <summary>Dry flavor.</summary>
        Dry = 1,
        /// <summary>Sweet flavor.</summary>
        Sweet = 2,
        /// <summary>Bitter flavor.</summary>
        Bitter = 3,
        /// <summary>Sour flavor.</summary>
        Sour = 4,
        /// <summary>No flavor / not applicable.</summary>
        None = 255
    }
}
