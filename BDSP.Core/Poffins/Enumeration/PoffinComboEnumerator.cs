using System;
using BDSP.Core.Berries;

namespace BDSP.Core.Poffins.Enumeration
{
    /// <summary>
    /// Enumerates unique 2-4 berry combinations from an arbitrary berry subset.
    /// Intended for UI-driven scenarios where the user filters berries first.
    /// </summary>
    /// <remarks>
    /// This uses a delegate callback for flexibility. For hot-path cooking,
    /// <see cref='Search.PoffinSearch'/> uses inlined loops for lower overhead.
    /// </remarks>
    public static class PoffinComboEnumerator
    {
        /// <summary>
        /// Enumerates all unique combinations of the specified size (2-4).
        /// </summary>
        /// <param name="source">The berry IDs to choose from.</param>
        /// <param name="choose">Number of berries per combination (2-4).</param>
        /// <param name="action">
        /// Callback invoked for each generated combination.
        /// The span is only valid for the duration of the call.
        /// </param>
        public static void ForEach(
            ReadOnlySpan<BerryId> source,
            int choose,
            Action<ReadOnlySpan<BerryId>> action)
        {
            if (choose < 2 || choose > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(choose), "Choose must be 2, 3, or 4.");
            }

            switch (choose)
            {
                case 2:
                    ForEach2(source, action);
                    break;
                case 3:
                    ForEach3(source, action);
                    break;
                case 4:
                    ForEach4(source, action);
                    break;
            }
        }

        /// <summary>
        /// Enumerates all 2-berry combinations (i &lt; j).
        /// </summary>
        private static void ForEach2(
            ReadOnlySpan<BerryId> source,
            Action<ReadOnlySpan<BerryId>> action)
        {
            Span<BerryId> buffer = stackalloc BerryId[2];

            for (int i = 0; i < source.Length - 1; i++)
            {
                buffer[0] = source[i];
                for (int j = i + 1; j < source.Length; j++)
                {
                    buffer[1] = source[j];
                    action(buffer);
                }
            }
        }

        /// <summary>
        /// Enumerates all 3-berry combinations (i &lt; j &lt; k).
        /// </summary>
        private static void ForEach3(
            ReadOnlySpan<BerryId> source,
            Action<ReadOnlySpan<BerryId>> action)
        {
            Span<BerryId> buffer = stackalloc BerryId[3];

            for (int i = 0; i < source.Length - 2; i++)
            {
                buffer[0] = source[i];
                for (int j = i + 1; j < source.Length - 1; j++)
                {
                    buffer[1] = source[j];
                    for (int k = j + 1; k < source.Length; k++)
                    {
                        buffer[2] = source[k];
                        action(buffer);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates all 4-berry combinations (i &lt; j &lt; k &lt; l).
        /// </summary>
        private static void ForEach4(
            ReadOnlySpan<BerryId> source,
            Action<ReadOnlySpan<BerryId>> action)
        {
            Span<BerryId> buffer = stackalloc BerryId[4];

            for (int i = 0; i < source.Length - 3; i++)
            {
                buffer[0] = source[i];
                for (int j = i + 1; j < source.Length - 2; j++)
                {
                    buffer[1] = source[j];
                    for (int k = j + 1; k < source.Length - 1; k++)
                    {
                        buffer[2] = source[k];
                        for (int l = k + 1; l < source.Length; l++)
                        {
                            buffer[3] = source[l];
                            action(buffer);
                        }
                    }
                }
            }
        }
    }
}

