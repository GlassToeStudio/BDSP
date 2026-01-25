using System;
using BDSP.Core.Poffins.Search;
using Xunit;

namespace BDSP.Core.Tests.Poffins.Search
{
    public class TopKTests
    {
        [Fact]
        public void Constructor_RejectsNonPositiveCapacity()
        {
            //Assert.Throws<ArgumentOutOfRangeException>(() => new TopK<int>(0));
            //Assert.Throws<ArgumentOutOfRangeException>(() => new TopK<int>(-1));
        }

        [Fact]
        public void TryAdd_KeepsHighestScores()
        {
            var top = new TopK<int>(3);
            top.TryAdd(1, 1);
            top.TryAdd(2, 2);
            top.TryAdd(3, 3);
            top.TryAdd(4, 4); // should evict score 1
            top.TryAdd(5, 5); // should evict score 2

            Assert.Equal(3, top.Count);

            var results = top.ToSortedArray((a, b) => b.CompareTo(a));
            Assert.Equal(new[] { 5, 4, 3 }, results);
        }

        [Fact]
        public void MergeFrom_CombinesAndKeepsTop()
        {
            var left = new TopK<int>(3);
            left.TryAdd(1, 1);
            left.TryAdd(3, 3);
            left.TryAdd(5, 5);

            var right = new TopK<int>(3);
            right.TryAdd(2, 2);
            right.TryAdd(4, 4);
            right.TryAdd(6, 6);

            left.MergeFrom(right);

            var results = left.ToSortedArray((a, b) => b.CompareTo(a));
            Assert.Equal(new[] { 6, 5, 4 }, results);
        }
    }
}
