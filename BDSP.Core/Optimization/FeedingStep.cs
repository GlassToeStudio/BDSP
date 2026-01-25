namespace BDSP.Core.Optimization
{
    /// <summary>
    /// One step in a feeding plan, including pre/post stats.
    /// </summary>
    public readonly struct FeedingStep
    {
        /// <summary>Index in the feeding sequence.</summary>
        public readonly int Index;
        /// <summary>Poffin and its recipe.</summary>
        public readonly PoffinWithRecipe Poffin;
        /// <summary>Stats before eating this poffin.</summary>
        public readonly ContestStats Before;
        /// <summary>Stats after eating this poffin.</summary>
        public readonly ContestStats After;

        public FeedingStep(int index, PoffinWithRecipe poffin, ContestStats before, ContestStats after)
        {
            Index = index;
            Poffin = poffin;
            Before = before;
            After = after;
        }
    }
}
