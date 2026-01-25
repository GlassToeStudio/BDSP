using BDSP.Core.Optimization.Core;

namespace BDSP.Core.Optimization.Search
{
    /// <summary>
    /// Options for contest stats search over poffin permutations.
    /// </summary>
    public readonly struct ContestStatsSearchOptions
    {
        /// <summary>Number of poffins per permutation (1-4).</summary>
        public readonly int Choose;
        /// <summary>When true, uses parallel execution for the outer loop.</summary>
        public readonly bool UseParallel;
        /// <summary>Optional parallelism limit (null = default).</summary>
        public readonly int? MaxDegreeOfParallelism;
        /// <summary>Starting stats (default = all zeros).</summary>
        public readonly ContestStats Start;

        public ContestStatsSearchOptions(
            int choose = 3,
            bool useParallel = true,
            int? maxDegreeOfParallelism = null,
            ContestStats start = default)
        {
            Choose = choose;
            UseParallel = useParallel;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            Start = start;
        }
    }
}
