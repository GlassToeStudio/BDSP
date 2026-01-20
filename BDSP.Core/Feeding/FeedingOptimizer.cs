namespace BDSP.Core.Feeding;

/// <summary>
/// Computes optimal multi-Poffin feeding plans under sheen constraints.
/// </summary>
public static class FeedingOptimizer
{
    /// <summary>
    /// Finds the optimal feeding plan that maximizes contest stats
    /// and secondarily maximizes sheen usage.
    /// </summary>
    public static FeedingPlan Optimize(
        ReadOnlySpan<Poffins.Poffin> candidates,
        FeedingOptions options)
    {
        var bestStates = new List<FeedingState>();

        var stack = new Stack<FeedingNode>();

        var initial = new FeedingNode(
            new FeedingState(0, default),
            lastIndex: -1,
            parent: null);

        stack.Push(initial);


        while (stack.Count > 0)
        {
            var node = stack.Pop();

            for (int i = node.LastIndex + 1; i < candidates.Length; i++)
            {
                var p = candidates[i];
                int newSheen = node.State.Sheen + p.Smoothness;

                if (newSheen > options.MaxSheen)
                    continue;

                var newStats =
                    node.State.Stats +
                    ContestStatsCalculator.FromPoffin(p);

                var newState = new FeedingState(newSheen, newStats);
                var newNode = new FeedingNode(newState, i, node);

                if (IsDominated(newNode, bestNodes))
                    continue;

                PruneDominated(newNode, bestNodes);
                bestNodes.Add(newNode);

                stack.Push(newNode);
            }
        }
        return BuildBestPlan(bestNodes, options, candidates);
    }

    private static bool IsDominated(
        FeedingNode candidate,
        List<FeedingNode> nodes)
    {
        foreach (var n in nodes)
        {
            if (n.State.Sheen <= candidate.State.Sheen &&
                n.State.Stats.Dominates(candidate.State.Stats))
                return true;
        }
        return false;
    }

    private static void PruneDominated(
        FeedingNode candidate,
        List<FeedingNode> nodes)
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (candidate.State.Sheen <= nodes[i].State.Sheen &&
                candidate.State.Stats.Dominates(nodes[i].State.Stats))
            {
                nodes.RemoveAt(i);
            }
        }
    }

    private static FeedingPlan BuildBestPlan(
        List<FeedingNode> nodes,
        FeedingOptions options,
        ReadOnlySpan<Poffins.Poffin> candidates)
    {
        FeedingNode best = nodes[0];

        foreach (var n in nodes)
        {
            int bestScore = options.Score(best.State.Stats);
            int curScore = options.Score(n.State.Stats);

            if (curScore > bestScore ||
               (curScore == bestScore &&
                n.State.Sheen > best.State.Sheen))
            {
                best = n;
            }
        }

        var poffins = new List<Poffins.Poffin>();

        for (var n = best; n.Parent != null; n = n.Parent)
            poffins.Add(candidates[n.LastIndex]);

        poffins.Reverse();
        return new FeedingPlan(poffins, best.State);
    }
}
