namespace BDSP.Core.Berries
{
    /// <summary>
    /// Sortable berry fields.
    /// </summary>
    public enum BerrySortField : byte
    {
        /// <summary>Sort by berry id.</summary>
        Id = 0,
        /// <summary>Sort by spicy value.</summary>
        Spicy,
        /// <summary>Sort by dry value.</summary>
        Dry,
        /// <summary>Sort by sweet value.</summary>
        Sweet,
        /// <summary>Sort by bitter value.</summary>
        Bitter,
        /// <summary>Sort by sour value.</summary>
        Sour,
        /// <summary>Sort by smoothness, lower is better.</summary>
        /// <remarks>
        /// <include file='BerryDocs.xml' path='doc/members/member[@name="T:SmoothToRare"]/*' />
        /// </remarks>
        Smoothness,
        /// <summary>Sort by rarity, lower is less rare.
        /// </summary>
        /// <remarks>
        /// <include file='BerryDocs.xml' path='doc/members/member[@name="T:SmoothToRare"]/*' />
        /// </remarks>
        Rarity,
        /// <summary>Sort by main flavor.</summary>
        /// <remarks>
        /// <include file='BerryDocs.xml' path='doc/members/member[@name="T:FlavorPreference"]/*' />
        /// </remarks>
        MainFlavor,
        /// <summary>Sort by secondary flavor.</summary>
        /// <remarks>
        /// <include file='BerryDocs.xml' path='doc/members/member[@name="T:FlavorPreference"]/*' />
        /// </remarks>
        SecondaryFlavor,
        /// <summary>Sort by main flavor value.</summary>
        /// <remarks>
        /// <include file='BerryDocs.xml' path='doc/members/member[@name="T:FlavorValueRange"]/*' />
        /// </remarks>
        MainFlavorValue,
        /// <summary>Sort by secondary flavor value.</summary>
        /// <remarks>
        /// <include file='BerryDocs.xml' path='doc/members/member[@name="T:FlavorValueRange"]/*' />
        /// </remarks>
        SecondaryFlavorValue,
        /// <summary>Sort by number of flavors (1-5).</summary>
        NumFlavors,
        /// <summary>Sort by display name.</summary>
        Name
    }
}
