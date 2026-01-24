using System;
using System.Collections.Generic;
using BDSP.Core.Berries;
using BDSP.Core.Cooking;
using Xunit;

namespace BDSP.Core.Tests.Cooking
{
    public class PoffinComboEnumeratorTests
    {
        [Fact]
        public void ForEach_Choose2_YieldsExpectedOrder()
        {
            BerryId[] source = { new BerryId(0), new BerryId(1), new BerryId(2), new BerryId(3) };
            List<string> results = new();

            PoffinComboEnumerator.ForEach(source, 2, combo =>
            {
                results.Add($"{combo[0].Value},{combo[1].Value}");
            });

            string[] expected =
            {
                "0,1",
                "0,2",
                "0,3",
                "1,2",
                "1,3",
                "2,3"
            };

            Assert.Equal(expected.Length, results.Count);
            Assert.Equal(expected, results);
        }

        [Fact]
        public void ForEach_Choose3_YieldsExpectedOrder()
        {
            BerryId[] source = { new BerryId(0), new BerryId(1), new BerryId(2), new BerryId(3) };
            List<string> results = new();

            PoffinComboEnumerator.ForEach(source, 3, combo =>
            {
                results.Add($"{combo[0].Value},{combo[1].Value},{combo[2].Value}");
            });

            string[] expected =
            {
                "0,1,2",
                "0,1,3",
                "0,2,3",
                "1,2,3"
            };

            Assert.Equal(expected.Length, results.Count);
            Assert.Equal(expected, results);
        }

        [Fact]
        public void ForEach_Choose4_YieldsSingleCombo()
        {
            BerryId[] source = { new BerryId(10), new BerryId(11), new BerryId(12), new BerryId(13) };
            List<string> results = new();

            PoffinComboEnumerator.ForEach(source, 4, combo =>
            {
                results.Add($"{combo[0].Value},{combo[1].Value},{combo[2].Value},{combo[3].Value}");
            });

            Assert.Single(results);
            Assert.Equal("10,11,12,13", results[0]);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void ForEach_InvalidChoose_Throws(int choose)
        {
            BerryId[] source = { new BerryId(0), new BerryId(1), new BerryId(2) };
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                PoffinComboEnumerator.ForEach(source, choose, _ => { }));
        }
    }
}
