using BDSP.Core.Berries;

namespace BDSP.Core.Cooking
{
    /// <summary>
    /// Precomputes unique berry combinations (2–4) into <see cref="PoffinComboBase"/> entries.
    /// This removes per-berry summation from the hot cooking loop.
    /// </summary>
    public static class PoffinComboTable
    {
        /// <summary>
        /// Total number of unique 2–4 berry combinations for the current berry table.
        /// </summary>
        public static readonly int Count = GetTotalComboCount(BerryTable.Count);

        /// <summary>
        /// All unique 2–4 berry combinations, precomputed from <see cref="BerryTable.BaseAll"/>.
        /// </summary>
        public static ReadOnlySpan<PoffinComboBase> All => AllCombos;

        /// <summary>
        /// Backing array for <see cref="All"/>; materialized once at type init.
        /// </summary>
        private static readonly PoffinComboBase[] AllCombos = CreateAllCombos();

        /// <summary>
        /// Builds all unique 2–4 berry combinations in deterministic ID order.
        /// </summary>
        private static PoffinComboBase[] CreateAllCombos()
        {
            ReadOnlySpan<BerryBase> berries = BerryTable.BaseAll;
            int berryCount = berries.Length;
            int total = GetTotalComboCount(berryCount);
            var combos = new PoffinComboBase[total];
            int index = 0;

            // 2-berry combinations (i < j) ensures uniqueness and deterministic order.
            for (int i = 0; i < berryCount - 1; i++)
            {
                ref readonly BerryBase a = ref berries[i];
                for (int j = i + 1; j < berryCount; j++)
                {
                    ref readonly BerryBase b = ref berries[j];
                    combos[index++] = CreateCombo(a, b);
                }
            }

            // 3-berry combinations (i < j < k).
            for (int i = 0; i < berryCount - 2; i++)
            {
                ref readonly BerryBase a = ref berries[i];
                for (int j = i + 1; j < berryCount - 1; j++)
                {
                    ref readonly BerryBase b = ref berries[j];
                    for (int k = j + 1; k < berryCount; k++)
                    {
                        ref readonly BerryBase c = ref berries[k];
                        combos[index++] = CreateCombo(a, b, c);
                    }
                }
            }

            // 4-berry combinations (i < j < k < l).
            for (int i = 0; i < berryCount - 3; i++)
            {
                ref readonly BerryBase a = ref berries[i];
                for (int j = i + 1; j < berryCount - 2; j++)
                {
                    ref readonly BerryBase b = ref berries[j];
                    for (int k = j + 1; k < berryCount - 1; k++)
                    {
                        ref readonly BerryBase c = ref berries[k];
                        for (int l = k + 1; l < berryCount; l++)
                        {
                            ref readonly BerryBase d = ref berries[l];
                            combos[index++] = CreateCombo(a, b, c, d);
                        }
                    }
                }
            }

            return combos;
        }

        /// <summary>
        /// Creates a 2-berry base by summing pre-weakened flavors and smoothness.
        /// </summary>
        private static PoffinComboBase CreateCombo(in BerryBase a, in BerryBase b)
        {
            return new PoffinComboBase(
                weakSpicySum: (short)(a.WeakSpicy + b.WeakSpicy),
                weakDrySum: (short)(a.WeakDry + b.WeakDry),
                weakSweetSum: (short)(a.WeakSweet + b.WeakSweet),
                weakBitterSum: (short)(a.WeakBitter + b.WeakBitter),
                weakSourSum: (short)(a.WeakSour + b.WeakSour),
                smoothnessSum: (ushort)(a.Smoothness + b.Smoothness),
                count: 2);
        }

        /// <summary>
        /// Creates a 3-berry base by summing pre-weakened flavors and smoothness.
        /// </summary>
        private static PoffinComboBase CreateCombo(in BerryBase a, in BerryBase b, in BerryBase c)
        {
            return new PoffinComboBase(
                weakSpicySum: (short)(a.WeakSpicy + b.WeakSpicy + c.WeakSpicy),
                weakDrySum: (short)(a.WeakDry + b.WeakDry + c.WeakDry),
                weakSweetSum: (short)(a.WeakSweet + b.WeakSweet + c.WeakSweet),
                weakBitterSum: (short)(a.WeakBitter + b.WeakBitter + c.WeakBitter),
                weakSourSum: (short)(a.WeakSour + b.WeakSour + c.WeakSour),
                smoothnessSum: (ushort)(a.Smoothness + b.Smoothness + c.Smoothness),
                count: 3);
        }

        /// <summary>
        /// Creates a 4-berry base by summing pre-weakened flavors and smoothness.
        /// </summary>
        private static PoffinComboBase CreateCombo(in BerryBase a, in BerryBase b, in BerryBase c, in BerryBase d)
        {
            return new PoffinComboBase(
                weakSpicySum: (short)(a.WeakSpicy + b.WeakSpicy + c.WeakSpicy + d.WeakSpicy),
                weakDrySum: (short)(a.WeakDry + b.WeakDry + c.WeakDry + d.WeakDry),
                weakSweetSum: (short)(a.WeakSweet + b.WeakSweet + c.WeakSweet + d.WeakSweet),
                weakBitterSum: (short)(a.WeakBitter + b.WeakBitter + c.WeakBitter + d.WeakBitter),
                weakSourSum: (short)(a.WeakSour + b.WeakSour + c.WeakSour + d.WeakSour),
                smoothnessSum: (ushort)(a.Smoothness + b.Smoothness + c.Smoothness + d.Smoothness),
                count: 4);
        }

        /// <summary>
        /// Computes the total number of 2–4 combinations for <paramref name="n"/> berries.
        /// </summary>
        private static int GetTotalComboCount(int n)
        {
            return Choose(n, 2) + Choose(n, 3) + Choose(n, 4);
        }

        /// <summary>
        /// Small, fixed-k combination counts; avoids BigInteger/overflow concerns for n=65.
        /// </summary>
        private static int Choose(int n, int k)
        {
            if (k < 0 || k > n)
            {
                return 0;
            }

            if (k == 0 || k == n)
            {
                return 1;
            }

            if (k == 1)
            {
                return n;
            }

            if (k == 2)
            {
                return n * (n - 1) / 2;
            }

            if (k == 3)
            {
                return n * (n - 1) * (n - 2) / 6;
            }

            if (k == 4)
            {
                return n * (n - 1) * (n - 2) * (n - 3) / 24;
            }

            return 0;
        }
    }
}
