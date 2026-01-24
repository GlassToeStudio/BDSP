using BenchmarkDotNet.Attributes;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Cooking;
using System.Threading;
using System.Threading.Tasks;

namespace BDSP.Core.Benchmarks
{
    [MemoryDiagnoser]
    public class SubsetCookingBenchmarks
    {
        [Params(10, 20, 30, 40, 50, 65)]
        public int SubsetSize;

        [Params(2, 3, 4)]
        public int Choose;

        private BerryId[] _ids = System.Array.Empty<BerryId>();
        private BerryBase[] _bases = System.Array.Empty<BerryBase>();
        private PoffinComboBase[] _precomputed = System.Array.Empty<PoffinComboBase>();

        [GlobalSetup]
        public void Setup()
        {
            int count = SubsetSize;
            _ids = new BerryId[count];
            for (int i = 0; i < count; i++)
            {
                _ids[i] = new BerryId((ushort)i);
            }

            _bases = BerryTable.BaseAll.ToArray();
            _precomputed = PoffinComboBuilder.CreateFromSubset(_ids);
        }

        [Benchmark(Baseline = true)]
        public int CookSubset_Direct()
        {
            int sum = 0;
            if (Choose == 2)
            {
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

            if (Choose == 3)
            {
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

            Span<BerryBase> buffer4 = stackalloc BerryBase[4];
            for (int i = 0; i < _ids.Length - 3; i++)
            {
                for (int j = i + 1; j < _ids.Length - 2; j++)
                {
                    for (int k = j + 1; k < _ids.Length - 1; k++)
                    {
                        for (int l = k + 1; l < _ids.Length; l++)
                        {
                            buffer4[0] = _bases[_ids[i].Value];
                            buffer4[1] = _bases[_ids[j].Value];
                            buffer4[2] = _bases[_ids[k].Value];
                            buffer4[3] = _bases[_ids[l].Value];
                            Poffin p = PoffinCooker.Cook(buffer4, 40, 0, 0);
                            sum += p.Level;
                        }
                    }
                }
            }
            return sum;
        }

        [Benchmark]
        public int CookSubset_Precomputed()
        {
            int sum = 0;
            for (int i = 0; i < _precomputed.Length; i++)
            {
                Poffin p = PoffinCooker.Cook(_precomputed[i], 40, 0, 0);
                sum += p.Level;
            }
            return sum;
        }

        [Benchmark]
        public int CookSubset_Direct_Parallel()
        {
            int sum = 0;
            BerryId[] ids = _ids;
            BerryBase[] bases = _bases;

            if (Choose == 2)
            {
                Parallel.For(0, ids.Length - 1,
                    () => 0,
                    (i, _, localSum) =>
                    {
                        Span<BerryBase> buffer = stackalloc BerryBase[2];
                        buffer[0] = bases[ids[i].Value];
                        for (int j = i + 1; j < ids.Length; j++)
                        {
                            buffer[1] = bases[ids[j].Value];
                            Poffin p = PoffinCooker.Cook(buffer, 40, 0, 0);
                            localSum += p.Level;
                        }
                        return localSum;
                    },
                    localSum => Interlocked.Add(ref sum, localSum));
                return sum;
            }

            if (Choose == 3)
            {
                Parallel.For(0, ids.Length - 2,
                    () => 0,
                    (i, _, localSum) =>
                    {
                        Span<BerryBase> buffer = stackalloc BerryBase[3];
                        buffer[0] = bases[ids[i].Value];
                        for (int j = i + 1; j < ids.Length - 1; j++)
                        {
                            buffer[1] = bases[ids[j].Value];
                            for (int k = j + 1; k < ids.Length; k++)
                            {
                                buffer[2] = bases[ids[k].Value];
                                Poffin p = PoffinCooker.Cook(buffer, 40, 0, 0);
                                localSum += p.Level;
                            }
                        }
                        return localSum;
                    },
                    localSum => Interlocked.Add(ref sum, localSum));
                return sum;
            }

            Parallel.For(0, ids.Length - 3,
                () => 0,
                (i, _, localSum) =>
                {
                    Span<BerryBase> buffer = stackalloc BerryBase[4];
                    buffer[0] = bases[ids[i].Value];
                    for (int j = i + 1; j < ids.Length - 2; j++)
                    {
                        buffer[1] = bases[ids[j].Value];
                        for (int k = j + 1; k < ids.Length - 1; k++)
                        {
                            buffer[2] = bases[ids[k].Value];
                            for (int l = k + 1; l < ids.Length; l++)
                            {
                                buffer[3] = bases[ids[l].Value];
                                Poffin p = PoffinCooker.Cook(buffer, 40, 0, 0);
                                localSum += p.Level;
                            }
                        }
                    }
                    return localSum;
                },
                localSum => Interlocked.Add(ref sum, localSum));
            return sum;
        }

        [Benchmark]
        public int CookSubset_Precomputed_Parallel()
        {
            int sum = 0;
            PoffinComboBase[] combos = _precomputed;
            Parallel.For(0, combos.Length,
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

        [Benchmark]
        public int BuildSubsetPrecompute()
        {
            return PoffinComboBuilder.CreateFromSubset(_ids).Length;
        }
    }
}
