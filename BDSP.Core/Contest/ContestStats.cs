using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BDSP.Core.Poffins;

namespace BDSP.Core.Contest
{
    public struct ContestStats
    {
        public byte Coolness;
        public byte Beauty;
        public byte Cuteness;
        public byte Cleverness;
        public byte Toughness;
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
    }

}
