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
        /// <summary>Maximum number of poffins to feed (0 = no cap).</summary>
        public readonly int MaxPoffins;
        /// <summary>When true, prunes dominated poffins before permutation search.</summary>
        public readonly bool PruneCandidates;
        /// <summary>Optional progress callback for long-running searches.</summary>
        public readonly Action<ContestSearchProgress>? Progress;
        /// <summary>Outer-loop interval for progress updates (0 disables).</summary>
        public readonly int ProgressInterval;

        public ContestStatsSearchOptions(
            int choose = 3,
            bool useParallel = true,
            int? maxDegreeOfParallelism = null,
            ContestStats start = default,
            int maxPoffins = 0,
            bool pruneCandidates = true,
            Action<ContestSearchProgress>? progress = null,
            int progressInterval = 64)
        {
            Choose = choose;
            UseParallel = useParallel;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            Start = start;
            MaxPoffins = maxPoffins;
            PruneCandidates = pruneCandidates;
            Progress = progress;
            ProgressInterval = progressInterval;
        }
    }

    /// <summary>
    /// Progress payload for contest stat searches (outer-loop based).
    /// </summary>
    public readonly struct ContestSearchProgress
    {
        public readonly int CompletedOuter;
        public readonly int TotalOuter;
        public readonly int Choose;
        public readonly bool IsParallel;
        public readonly int CandidateCount;

        public ContestSearchProgress(int completedOuter, int totalOuter, int choose, bool isParallel, int candidateCount)
        {
            CompletedOuter = completedOuter;
            TotalOuter = totalOuter;
            Choose = choose;
            IsParallel = isParallel;
            CandidateCount = candidateCount;
        }
    }
}
