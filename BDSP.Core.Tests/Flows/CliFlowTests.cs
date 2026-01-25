using System;
using System.Collections.Generic;
using BDSP.Core.Berries;
using BDSP.Core.Optimization.Core;
using BDSP.Core.Optimization.Search;
using BDSP.Core.Poffins;
using Xunit;

namespace BDSP.Core.Tests.Flows
{
    public class CliFlowTests
    {
        [Fact]
        public void ContestSearch_PipelineFromBerries_ProducesResults()
        {
            var candidateOptions = new PoffinCandidateOptions(chooseList: new[] { 2 }, cookTimeSeconds: 40);
            var contestOptions = new ContestStatsSearchOptions(choose: 2, useParallel: false);
            var searchOptions = FeedingSearchOptions.Default;

            ContestStatsResult[] results = OptimizationPipeline.RunContestSearch(
                berryOptions: default,
                candidateOptions: in candidateOptions,
                candidateTopK: 50,
                contestOptions: in contestOptions,
                scoringOptions: in searchOptions,
                topK: 10,
                dedup: true);

            Assert.NotEmpty(results);
        }

    }
}
