using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries
{
    /// <summary>
    /// Berry names are indexed by by <see cref="BerryId.Value"/>.
    /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:BerryIdTable"]/*' />
    /// </summary>
    /// <remarks>
    /// Berry names are sorted alphabetically by default.
    /// </remarks>
    public static class BerryNames
    {
        private static readonly string[] Names =
        {
            "Aguav Berry",
            "Apicot Berry",
            "Aspear Berry",
            "Babiri Berry",
            "Belue Berry",
            "Bluk Berry",
            "Charti Berry",
            "Cheri Berry",
            "Chesto Berry",
            "Chilan Berry",
            "Chople Berry",
            "Coba Berry",
            "Colbur Berry",
            "Cornn Berry",
            "Custap Berry",
            "Durin Berry",
            "Enigma Berry",
            "Figy Berry",
            "Ganlon Berry",
            "Grepa Berry",
            "Haban Berry",
            "Hondew Berry",
            "Iapapa Berry",
            "Jaboca Berry",
            "Kasib Berry",
            "Kebia Berry",
            "Kelpsy Berry",
            "Lansat Berry",
            "Leppa Berry",
            "Liechi Berry",
            "Lum Berry",
            "Mago Berry",
            "Magost Berry",
            "Micle Berry",
            "Nanab Berry",
            "Nomel Berry",
            "Occa Berry",
            "Oran Berry",
            "Pamtre Berry",
            "Passho Berry",
            "Payapa Berry",
            "Pecha Berry",
            "Persim Berry",
            "Petaya Berry",
            "Pinap Berry",
            "Pomeg Berry",
            "Qualot Berry",
            "Rabuta Berry",
            "Rawst Berry",
            "Razz Berry",
            "Rindo Berry",
            "Roseli Berry",
            "Rowap Berry",
            "Salac Berry",
            "Shuca Berry",
            "Sitrus Berry",
            "Spelon Berry",
            "Starf Berry",
            "Tamato Berry",
            "Tanga Berry",
            "Wacan Berry",
            "Watmel Berry",
            "Wepear Berry",
            "Wiki Berry",
            "Yache Berry",
        };

        /// <summary>
        /// Returns the display name for the given berry ID.
        /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:BerryIdTable"]/*' />
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName(BerryId id) => Names[id.Value];
    }
}
