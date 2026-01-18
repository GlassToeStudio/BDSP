using BDSP.Core.Poffins;

namespace BDSP.Core.Selection
{
    public interface IPoffinComparer
    {
        /// <summary>
        /// Returns true if A is strictly better than B.
        /// </summary>
        bool IsBetter(in Poffin a, in Poffin b);
    }
}