using System;
using BDSP.Core.Berries;

var ids = new[] { new BerryId(18), new BerryId(16), new BerryId(52) };
foreach (var id in ids)
{
    ref readonly var berry = ref BerryTable.Get(id);
    Console.WriteLine(BDSP.Tools.BerryAnsiFormatter.Format(berry));
}
