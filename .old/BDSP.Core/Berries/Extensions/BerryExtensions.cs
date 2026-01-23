using System.Runtime.CompilerServices;

using BDSP.Core.Berries.Data;

namespace BDSP.Core.Berries.Extensions;

/// <summary>
/// Provides convenient extension methods for berry-related data structures.
/// This class acts as a bridge between the core data structs and their string representations.
/// </summary>
public static class BerryExtensions
{
    /// <summary>
    /// Gets the canonical name for a given <see cref="BerryId"/>.
    /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:BerryIdTable"]/*' />
    /// </summary>
    /// <param name="id">The berry identifier.</param>
    /// <returns>The string name of the berry.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this BerryId id)
        => BerryNames.GetName(id);

    /// <summary>
    /// Gets the canonical name for a given <see cref="Berry"/>.
    /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:BerryIdTable"]/*' />
    /// </summary>
    /// <param name="berry">The berry instance.</param>
    /// <returns>The string name of the berry.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this Berry berry)
        => BerryNames.GetName(berry.Id);
}
