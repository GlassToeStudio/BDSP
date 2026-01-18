using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDSP.Core.Poffins;

namespace BDSP.Core.Selection
{
    public sealed class LevelThenSmoothnessComparer : IPoffinComparer
    {
        public bool IsBetter(in Poffin a, in Poffin b)
        {
            if (a.Level != b.Level)
                return a.Level > b.Level;

            if (a.Smoothness != b.Smoothness)
                return a.Smoothness < b.Smoothness; // lower smoothness is better

            return a.Type > b.Type;
        }
    }

}