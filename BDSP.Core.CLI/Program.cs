using System;
using BDSP.Core.Berries;

for (ushort i = 0; i < 65; i++)
{
    ref readonly var berry = ref BerryTable.Get(new BerryId(i));
    Console.WriteLine(berry);
    Console.WriteLine(BDSP.Tools.BerryAnsiFormatter.Format(berry));
}
