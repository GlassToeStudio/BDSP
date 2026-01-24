using System;
using BenchmarkDotNet.Attributes;
using BDSP.Core.Berries;
using BDSP.Core.Cooking;
using BDSP.Core.Poffins;

namespace BDSP.Core.Benchmarks
{
    [MemoryDiagnoser]
    public class CookingBenchmarks
    {
        private BerryId[] _ids = Array.Empty<BerryId>();
        private PoffinComboBase[] _allCombos = Array.Empty<PoffinComboBase>();
        private int _count2;
        private int _count3;
        private int _count4;
        private int _start3;
        private int _start4;

        [GlobalSetup]
        public void Setup()
        {
            int n = BerryTable.Count;
            _count2 = Choose(n, 2);
            _count3 = Choose(n, 3);
            _count4 = Choose(n, 4);
            _start3 = _count2;
            _start4 = _count2 + _count3;

            _ids = new BerryId[n];
            for (int i = 0; i < n; i++)
            {
                _ids[i] = new BerryId((ushort)i);
            }

            _allCombos = PoffinComboTable.All.ToArray();
        }

        [Benchmark(Baseline = true)]
        public int CookAllCombos2_FromComboBase()
        {
            int sum = 0;
            for (int i = 0; i < _count2; i++)
            {
                Poffin p = PoffinCooker.Cook(_allCombos[i], 40, 0, 0);
                sum += p.Level;
            }
            return sum;
        }

        [Benchmark]
        public int CookAllCombos2_FromSpan()
        {
            int sum = 0;
            ReadOnlySpan<BerryBase> bases = BerryTable.BaseAll;
            for (int i = 0; i < _ids.Length - 1; i++)
            {
                for (int j = i + 1; j < _ids.Length; j++)
                {
                    Span<BerryBase> berries = stackalloc BerryBase[2];
                    berries[0] = bases[_ids[i].Value];
                    berries[1] = bases[_ids[j].Value];
                    Poffin p = PoffinCooker.Cook(berries, 40, 0, 0);
                    sum += p.Level;
                }
            }
            return sum;
        }

        [Benchmark]
        public int CookAllCombos3_FromComboBase()
        {
            int sum = 0;
            int end = _start3 + _count3;
            for (int i = _start3; i < end; i++)
            {
                Poffin p = PoffinCooker.Cook(_allCombos[i], 40, 0, 0);
                sum += p.Level;
            }
            return sum;
        }

        [Benchmark]
        public int CookAllCombos3_FromSpan()
        {
            int sum = 0;
            ReadOnlySpan<BerryBase> bases = BerryTable.BaseAll;
            for (int i = 0; i < _ids.Length - 2; i++)
            {
                for (int j = i + 1; j < _ids.Length - 1; j++)
                {
                    for (int k = j + 1; k < _ids.Length; k++)
                    {
                        Span<BerryBase> berries = stackalloc BerryBase[3];
                        berries[0] = bases[_ids[i].Value];
                        berries[1] = bases[_ids[j].Value];
                        berries[2] = bases[_ids[k].Value];
                        Poffin p = PoffinCooker.Cook(berries, 40, 0, 0);
                        sum += p.Level;
                    }
                }
            }
            return sum;
        }

        [Benchmark]
        public int CookAllCombos4_FromComboBase()
        {
            int sum = 0;
            int end = _start4 + _count4;
            for (int i = _start4; i < end; i++)
            {
                Poffin p = PoffinCooker.Cook(_allCombos[i], 40, 0, 0);
                sum += p.Level;
            }
            return sum;
        }

        [Benchmark]
        public int CookAllCombos4_FromSpan()
        {
            int sum = 0;
            ReadOnlySpan<BerryBase> bases = BerryTable.BaseAll;
            for (int i = 0; i < _ids.Length - 3; i++)
            {
                for (int j = i + 1; j < _ids.Length - 2; j++)
                {
                    for (int k = j + 1; k < _ids.Length - 1; k++)
                    {
                        for (int l = k + 1; l < _ids.Length; l++)
                        {
                            Span<BerryBase> berries = stackalloc BerryBase[4];
                            berries[0] = bases[_ids[i].Value];
                            berries[1] = bases[_ids[j].Value];
                            berries[2] = bases[_ids[k].Value];
                            berries[3] = bases[_ids[l].Value];
                            Poffin p = PoffinCooker.Cook(berries, 40, 0, 0);
                            sum += p.Level;
                        }
                    }
                }
            }
            return sum;
        }

        private static int Choose(int n, int k)
        {
            if (k == 2) return n * (n - 1) / 2;
            if (k == 3) return n * (n - 1) * (n - 2) / 6;
            if (k == 4) return n * (n - 1) * (n - 2) * (n - 3) / 24;
            return 0;
        }
    }
}
