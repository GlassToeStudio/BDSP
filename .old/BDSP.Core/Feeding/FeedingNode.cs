using BDSP.Core.Contest;

namespace BDSP.Core.Feeding
{
    internal sealed class FeedingNode
    {
        public FeedingState State { get; }
        public int LastIndex { get; }
        public FeedingNode? Parent { get; }

        public FeedingNode(FeedingState state, int lastIndex, FeedingNode? parent)
        {
            State = state;
            LastIndex = lastIndex;
            Parent = parent;
        }
    }

}
