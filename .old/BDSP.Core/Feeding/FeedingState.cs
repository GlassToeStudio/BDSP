using BDSP.Core.Contest;

namespace BDSP.Core.Feeding
{

    /// <summary>
    /// Represents the cumulative state of feeding Poffins to a Pokémon.
    /// </summary>
    public readonly struct FeedingState
    {
        /// <summary>Total accumulated sheen.</summary>
        public int Sheen { get; }

        /// <summary>Accumulated contest stats.</summary>
        public ContestStats Stats { get; }

        public FeedingState(int sheen, Contest.ContestStats stats)
        {
            Sheen = sheen;
            Stats = stats;
        }

        public override string ToString()
        {
            return $"Sheen {Sheen}, {Stats}";
        }

    }
}
