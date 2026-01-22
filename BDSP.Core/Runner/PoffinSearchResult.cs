using BDSP.Core.Poffins;

namespace BDSP.Core.Runner
{
    /// <summary>
    /// Result of a Poffin search containing the top-ranked Poffins.
    /// </summary>
    public readonly struct PoffinSearchResult
    {
        /// <summary>
        /// Top-ranked Poffins in descending order according to the comparer.
        /// </summary>
        public readonly Poffin[] TopPoffins;

        public PoffinSearchResult(Poffin[] topPoffins)
        {
            TopPoffins = topPoffins;
        }
    }
}
