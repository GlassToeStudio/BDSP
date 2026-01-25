using System;
using System.Collections.Generic;
using BDSP.Core.Optimization.Enumeration;
using Xunit;

namespace BDSP.Core.Tests.Optimization
{
    public class PoffinPermutationEnumeratorTests
    {
        [Fact]
        public void ForEach_Choose2_YieldsExpectedCount()
        {
            int[] source = { 1, 2, 3 };
            var results = new List<string>();

            PoffinPermutationEnumerator.ForEach<int>(source, 2, combo =>
            {
                results.Add($"{combo[0]}{combo[1]}");
            });

            // 3P2 = 6
            Assert.Equal(6, results.Count);
            Assert.Contains("12", results);
            Assert.Contains("21", results);
            Assert.Contains("13", results);
            Assert.Contains("31", results);
            Assert.Contains("23", results);
            Assert.Contains("32", results);
        }

        [Fact]
        public void ForEach_Choose3_YieldsExpectedCount()
        {
            int[] source = { 1, 2, 3, 4 };
            int count = 0;

            PoffinPermutationEnumerator.ForEach<int>(source, 3, _ =>
            {
                count++;
            });

            // 4P3 = 24
            Assert.Equal(24, count);
        }

        [Fact]
        public void ForEach_InvalidChoose_Throws()
        {
            int[] source = { 1, 2, 3 };
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                PoffinPermutationEnumerator.ForEach<int>(source, 0, _ => { }));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                PoffinPermutationEnumerator.ForEach<int>(source, 4, _ => { }));
        }
    }
}
