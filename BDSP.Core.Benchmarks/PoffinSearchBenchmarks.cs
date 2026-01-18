using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Runner;
using BDSP.Core.Selection;

namespace BDSP.Core.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class PoffinSearchBenchmarks
{
    private BerryId[] _berryPool = null!;
    private IPoffinComparer _comparer = null!;

    [Params(2, 3, 4)]
    public int BerriesPerPoffin;

    [Params(10, 50)]
    public int TopK;

    [GlobalSetup]
    public void Setup()
    {
        _berryPool = new BerryId[BerryTable.Count];
        for (ushort i = 0; i < BerryTable.Count; i++)
            _berryPool[i] = new BerryId(i);

        _comparer = new LevelThenSmoothnessComparer();
    }

    [Benchmark]
    public PoffinSearchResult FullSearch()
    {
        return PoffinSearchRunner.Run(
            berryPool: _berryPool,
            berriesPerPoffin: BerriesPerPoffin,
            topK: TopK,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 9,
            comparer: _comparer,
            predicate: static (in Poffin p) => p.Type != PoffinType.Foul,
            maxDegreeOfParallelism: Environment.ProcessorCount
        );
    }
}
