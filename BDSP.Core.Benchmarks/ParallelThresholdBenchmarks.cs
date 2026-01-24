using BenchmarkDotNet.Attributes;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Cooking;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDSP.Core.Benchmarks
{
    [MemoryDiagnoser]
    public class ParallelThresholdBenchmarks
    {
        [Params(100, 500, 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 400000, 700000)]
        public int ComboCount;

        private PoffinComboBase[] _combos = Array.Empty<PoffinComboBase>();
        private int _count;

        [GlobalSetup]
        public void Setup()
        {
            _combos = PoffinComboTable.All.ToArray();
            _count = Math.Min(ComboCount, _combos.Length);
        }

        [Benchmark(Baseline = true)]
        public int CookFirstN_Sequential()
        {
            int sum = 0;
            for (int i = 0; i < _count; i++)
            {
                Poffin p = PoffinCooker.Cook(_combos[i], 40, 0, 0);
                sum += p.Level;
            }
            return sum;
        }

        [Benchmark]
        public int CookFirstN_Parallel()
        {
            int sum = 0;
            PoffinComboBase[] combos = _combos;
            Parallel.For(0, _count,
                () => 0,
                (i, _, localSum) =>
                {
                    Poffin p = PoffinCooker.Cook(combos[i], 40, 0, 0);
                    localSum += p.Level;
                    return localSum;
                },
                localSum => Interlocked.Add(ref sum, localSum));
            return sum;
        }
    }
}
