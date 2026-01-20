using System;

namespace BDSP.Core.Contest;

/// <summary>
/// Computes contest stat effects from a cooked Poffin
/// according to BDSP (Generation VIII) rules.
/// </summary>
public static class ContestStatsCalculator
{
    /// <summary>
    /// Converts a <see cref="Poffins.Poffin"/> into contest stat gains.
    /// </summary>
    /// <param name="poffin">The cooked poffin.</param>
    /// <returns>The resulting contest stats.</returns>
    public static ContestStats FromPoffin(in Poffins.Poffin poffin)
    {
        // In BDSP, contest stat gain equals the flavor values directly.
        // Negative or zero flavors contribute nothing.
        return new ContestStats(
            cool: poffin.Spicy > 0 ? poffin.Spicy : (byte)0,
            beauty: poffin.Dry > 0 ? poffin.Dry : (byte)0,
            cute: poffin.Sweet > 0 ? poffin.Sweet : (byte)0,
            smart: poffin.Bitter > 0 ? poffin.Bitter : (byte)0,
            tough: poffin.Sour > 0 ? poffin.Sour : (byte)0
        );
    }
}
