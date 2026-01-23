using BDSP.Core.Poffins;

namespace BDSP.Core.Selection
{

    /// <summary>
    /// Lightweight allocation-free predicate used to filter Poffins before ranking.
    /// </summary>
    /// <param name="poffin">The Poffin to evaluate.</param>
    /// <returns>
    /// True if the Poffin should be considered; otherwise false.
    /// </returns>
    public delegate bool PoffinPredicate(in Poffin poffin);
}
