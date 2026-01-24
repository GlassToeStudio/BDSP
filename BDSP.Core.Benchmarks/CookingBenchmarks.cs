using System;
using System.Threading;
using System.Threading.Tasks;
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
        private BerryBase[] _bases = Array.Empty<BerryBase>();
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

            _bases = BerryTable.BaseAll.ToArray();
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
            Span<BerryBase> buffer = stackalloc BerryBase[2];
            for (int i = 0; i < _ids.Length - 1; i++)
            {
                for (int j = i + 1; j < _ids.Length; j++)
                {
                    buffer[0] = _bases[_ids[i].Value];
                    buffer[1] = _bases[_ids[j].Value];
                    Poffin p = PoffinCooker.Cook(buffer, 40, 0, 0);
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
            Span<BerryBase> buffer = stackalloc BerryBase[3];
            for (int i = 0; i < _ids.Length - 2; i++)
            {
                for (int j = i + 1; j < _ids.Length - 1; j++)
                {
                    for (int k = j + 1; k < _ids.Length; k++)
                    {
                        buffer[0] = _bases[_ids[i].Value];
                        buffer[1] = _bases[_ids[j].Value];
                        buffer[2] = _bases[_ids[k].Value];
                        Poffin p = PoffinCooker.Cook(buffer, 40, 0, 0);
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
            Span<BerryBase> buffer = stackalloc BerryBase[4];
            for (int i = 0; i < _ids.Length - 3; i++)
            {
                for (int j = i + 1; j < _ids.Length - 2; j++)
                {
                    for (int k = j + 1; k < _ids.Length - 1; k++)
                    {
                        for (int l = k + 1; l < _ids.Length; l++)
                        {
                            buffer[0] = _bases[_ids[i].Value];
                            buffer[1] = _bases[_ids[j].Value];
                            buffer[2] = _bases[_ids[k].Value];
                            buffer[3] = _bases[_ids[l].Value];
                            Poffin p = PoffinCooker.Cook(buffer, 40, 0, 0);
                            sum += p.Level;
                        }
                    }
                }
            }
            return sum;
        }

        [Benchmark]
        public long CookAllCombos2_FromComboBase_Parallel()
        {
            long total = 0;
            Parallel.For(
                0,
                _count2,
                () => 0L,
                (i, _, local) =>
                {
                    local += PoffinCooker.Cook(_allCombos[i], 40, 0, 0).Level;
                    return local;
                },
                local => Interlocked.Add(ref total, local));
            return total;
        }

        [Benchmark]
        public long CookAllCombos2_FromSpan_Parallel()
        {
            long total = 0;
            Parallel.For(
                0,
                _ids.Length - 1,
                () => new SpanCookLocal(2),
                (i, _, local) =>
                {
                    for (int j = i + 1; j < _ids.Length; j++)
                    {
                        local.Buffer[0] = _bases[_ids[i].Value];
                        local.Buffer[1] = _bases[_ids[j].Value];
                        local.Sum += PoffinCooker.Cook(local.Buffer, 40, 0, 0).Level;
                    }
                    return local;
                },
                local => Interlocked.Add(ref total, local.Sum));
            return total;
        }

        [Benchmark]
        public long CookAllCombos3_FromComboBase_Parallel()
        {
            long total = 0;
            int end = _start3 + _count3;
            Parallel.For(
                _start3,
                end,
                () => 0L,
                (i, _, local) =>
                {
                    local += PoffinCooker.Cook(_allCombos[i], 40, 0, 0).Level;
                    return local;
                },
                local => Interlocked.Add(ref total, local));
            return total;
        }

        [Benchmark]
        public long CookAllCombos3_FromSpan_Parallel()
        {
            long total = 0;
            Parallel.For(
                0,
                _ids.Length - 2,
                () => new SpanCookLocal(3),
                (i, _, local) =>
                {
                    for (int j = i + 1; j < _ids.Length - 1; j++)
                    {
                        for (int k = j + 1; k < _ids.Length; k++)
                        {
                            local.Buffer[0] = _bases[_ids[i].Value];
                            local.Buffer[1] = _bases[_ids[j].Value];
                            local.Buffer[2] = _bases[_ids[k].Value];
                            local.Sum += PoffinCooker.Cook(local.Buffer, 40, 0, 0).Level;
                        }
                    }
                    return local;
                },
                local => Interlocked.Add(ref total, local.Sum));
            return total;
        }

        [Benchmark]
        public long CookAllCombos4_FromComboBase_Parallel()
        {
            long total = 0;
            int end = _start4 + _count4;
            Parallel.For(
                _start4,
                end,
                () => 0L,
                (i, _, local) =>
                {
                    local += PoffinCooker.Cook(_allCombos[i], 40, 0, 0).Level;
                    return local;
                },
                local => Interlocked.Add(ref total, local));
            return total;
        }

        [Benchmark]
        public long CookAllCombos4_FromSpan_Parallel()
        {
            long total = 0;
            Parallel.For(
                0,
                _ids.Length - 3,
                () => new SpanCookLocal(4),
                (i, _, local) =>
                {
                    for (int j = i + 1; j < _ids.Length - 2; j++)
                    {
                        for (int k = j + 1; k < _ids.Length - 1; k++)
                        {
                            for (int l = k + 1; l < _ids.Length; l++)
                            {
                                local.Buffer[0] = _bases[_ids[i].Value];
                                local.Buffer[1] = _bases[_ids[j].Value];
                                local.Buffer[2] = _bases[_ids[k].Value];
                                local.Buffer[3] = _bases[_ids[l].Value];
                                local.Sum += PoffinCooker.Cook(local.Buffer, 40, 0, 0).Level;
                            }
                        }
                    }
                    return local;
                },
                local => Interlocked.Add(ref total, local.Sum));
            return total;
        }

        private sealed class SpanCookLocal
        {
            public long Sum;
            public BerryBase[] Buffer;

            public SpanCookLocal(int size)
            {
                Sum = 0;
                Buffer = new BerryBase[size];
            }
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
