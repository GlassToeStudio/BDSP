using System;
using System.Buffers;

namespace BDSP.Core.Optimization.Enumeration
{
    /// <summary>
    /// Enumerates ordered permutations of candidate items without repetition.
    /// Intended for feeding-plan exploration on pre-filtered candidate sets.
    /// </summary>
    public static class PoffinPermutationEnumerator
    {
        /// <summary>
        /// Enumerates all permutations of length <paramref name="choose"/> from <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Candidate items.</param>
        /// <param name="choose">Permutation length (1..source.Length).</param>
        /// <param name="action">Callback invoked per permutation (span is valid only during the call).</param>
        public static void ForEach<T>(
            ReadOnlySpan<T> source,
            int choose,
            Action<ReadOnlySpan<T>> action)
        {
            if (choose < 1 || choose > source.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(choose), "Choose must be between 1 and source length.");
            }

            if (source.Length == 0)
            {
                return;
            }

            int n = source.Length;
            int[] indices = ArrayPool<int>.Shared.Rent(choose);
            bool[] used = ArrayPool<bool>.Shared.Rent(n);
            try
            {
                Array.Clear(used, 0, n);
                Permute(source, choose, 0, indices, used, action);
            }
            finally
            {
                ArrayPool<int>.Shared.Return(indices);
                ArrayPool<bool>.Shared.Return(used);
            }
        }

        private static void Permute<T>(
            ReadOnlySpan<T> source,
            int choose,
            int depth,
            int[] indices,
            bool[] used,
            Action<ReadOnlySpan<T>> action)
        {
            if (depth == choose)
            {
                T[] buffer = ArrayPool<T>.Shared.Rent(choose);
                try
                {
                    for (int i = 0; i < choose; i++)
                    {
                        buffer[i] = source[indices[i]];
                    }
                    action(buffer.AsSpan(0, choose));
                }
                finally
                {
                    ArrayPool<T>.Shared.Return(buffer);
                }
                return;
            }

            for (int i = 0; i < source.Length; i++)
            {
                if (used[i])
                {
                    continue;
                }

                used[i] = true;
                indices[depth] = i;
                Permute(source, choose, depth + 1, indices, used, action);
                used[i] = false;
            }
        }
    }
}
