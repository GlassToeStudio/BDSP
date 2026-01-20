using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDSP.Core.Berries
{
    public readonly struct BerryId
    {
        public readonly ushort Value;
        public BerryId(ushort value) => Value = value;
    }
}
