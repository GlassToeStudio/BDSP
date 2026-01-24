using System;
using BDSP.Core.Berries;

namespace BDSP.Core.Poffins.Cooking
{
    /// <summary>
    /// Builds precomputed combo bases for a user-selected subset of berries.
    /// Use when the same subset will be cooked multiple times (e.g., re-ranking or plan search).
    /// </summary>
    public static class PoffinComboBuilder
    {
        /// <summary>
        /// Creates precomputed combo bases (2–4) from a subset of berry IDs.
        /// </summary>
        /// <param name="ids">Subset of berry IDs (unique).</param>
        /// <returns>Array of precomputed combo bases for the subset.</returns>
        public static PoffinComboBase[] CreateFromSubset(ReadOnlySpan<BerryId> ids)
        {
            if (ids.Length < 2)
            {
                return Array.Empty<PoffinComboBase>();
            }

            ReadOnlySpan<BerryBase> bases = BerryTable.BaseAll;
            var count = GetTotalComboCount(ids.Length);
            var combos = new PoffinComboBase[count];
            int index = 0;

            // 2-berry combinations
            for (int i = 0; i < ids.Length - 1; i++)
            {
                ref readonly BerryBase a = ref bases[ids[i].Value];
                for (int j = i + 1; j < ids.Length; j++)
                {
                    ref readonly BerryBase b = ref bases[ids[j].Value];
                    combos[index++] = CreateCombo(a, b);
                }
            }

            // 3-berry combinations
            for (int i = 0; i < ids.Length - 2; i++)
            {
                ref readonly BerryBase a = ref bases[ids[i].Value];
                for (int j = i + 1; j < ids.Length - 1; j++)
                {
                    ref readonly BerryBase b = ref bases[ids[j].Value];
                    for (int k = j + 1; k < ids.Length; k++)
                    {
                        ref readonly BerryBase c = ref bases[ids[k].Value];
                        combos[index++] = CreateCombo(a, b, c);
                    }
                }
            }

            // 4-berry combinations
            for (int i = 0; i < ids.Length - 3; i++)
            {
                ref readonly BerryBase a = ref bases[ids[i].Value];
                for (int j = i + 1; j < ids.Length - 2; j++)
                {
                    ref readonly BerryBase b = ref bases[ids[j].Value];
                    for (int k = j + 1; k < ids.Length - 1; k++)
                    {
                        ref readonly BerryBase c = ref bases[ids[k].Value];
                        for (int l = k + 1; l < ids.Length; l++)
                        {
                            ref readonly BerryBase d = ref bases[ids[l].Value];
                            combos[index++] = CreateCombo(a, b, c, d);
                        }
                    }
                }
            }

            return combos;
        }

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

        private static int GetTotalComboCount(int n)
        {
            if (n < 2) return 0;
            return Choose(n, 2) + Choose(n, 3) + Choose(n, 4);
        }

        private static int Choose(int n, int k)
        {
            if (k == 2) return n * (n - 1) / 2;
            if (k == 3) return n * (n - 1) * (n - 2) / 6;
            if (k == 4) return n * (n - 1) * (n - 2) * (n - 3) / 24;
            return 0;
        }
    }
}
