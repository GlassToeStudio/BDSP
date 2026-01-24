using BenchmarkDotNet.Attributes;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Poffins.Cooking;

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
        public int BuildSubsetPrecompute()
        {
            return PoffinComboBuilder.CreateFromSubset(_ids).Length;
        }
    }
}
