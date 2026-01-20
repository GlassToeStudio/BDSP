using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BDSP.Core.Poffins;

namespace BDSP.Core.Contest
{
    /*
        Condition	Flavor	Color
        Coolness	Spicy	Red
        Beauty	    Dry	    Blue
        Cuteness	Sweet	Pink
        Cleverness	Bitter	Green
        Toughness	Sour	Yellow
    */
    /// <summary>
    /// Represents contest stat gains from consuming a Poffin.
    /// </summary>
    public struct ContestStats
    {
        /// <summary>Cool contest stat.</summary>
        public byte Coolness;
        /// <summary>Beautiful contest stat.</summary>
        public byte Beauty;
        /// <summary>Cuteness contest stat. </summary>
        public byte Cuteness;
        /// <summary>Cleverness contest stat.</summary>
        public byte Cleverness;
        /// <summary>Tough contest stat.</summary>
        public byte Toughness;
        /// <summary>Sheen Pokemon stat.</summary>
        public byte Sheen;

        public byte PerfectCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Feed(in Poffin poffin)
        {
            if (Sheen >= 255)
                return;

            Sheen = (byte)Math.Min(255, Sheen + poffin.Smoothness);

            Coolness = (byte)Math.Min(255, Coolness + poffin.Spicy);
            Beauty = (byte)Math.Min(255, Beauty + poffin.Dry);
            Cuteness = (byte)Math.Min(255, Cuteness + poffin.Sweet);
            Cleverness = (byte)Math.Min(255, Cleverness + poffin.Bitter);
            Toughness = (byte)Math.Min(255, Toughness + poffin.Sour);

            // PerfectCount recomputed only when needed
        }

        /// <summary>
        /// Returns true if this stat vector dominates the other.
        /// </summary>
        public bool Dominates(ContestStats other)
        {
            return Cool >= other.Cool &&
                   Beauty >= other.Beauty &&
                   Cute >= other.Cute &&
                   Smart >= other.Smart &&
                   Tough >= other.Tough;
        }

        /// <summary>
        /// Adds two contest stat vectors.
        /// </summary>
        public static ContestStats operator +(
            ContestStats a,
            ContestStats b)
        {
            return new ContestStats(
                (byte)(a.Cool + b.Cool),
                (byte)(a.Beauty + b.Beauty),
                (byte)(a.Cute + b.Cute),
                (byte)(a.Smart + b.Smart),
                (byte)(a.Tough + b.Tough));
        }
    }
}
