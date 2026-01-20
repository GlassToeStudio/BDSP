using BDSP.Core.Poffins;

namespace BDSP.Core.Selection
{
    /// <summary>
    /// Defines a comparison strategy for ranking Poffins.
    /// </summary>
    public interface IPoffinComparer
    {
        /// <summary>
        /// Determines whether <paramref name="a"/> is better than <paramref name="b"/>.
        /// </summary>
        bool IsBetter(in Poffin a, in Poffin b);
    }
}