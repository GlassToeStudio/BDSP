using System;
using BDSP.Core.Berries;

namespace BDSP.Core.Berries;

/// <summary>
/// Generates combinations of berries without allocation.
/// </summary>
public static class BerryCombinations
{
    /// <summary>
    /// Enumerates all combinations of the specified size from a berry pool.
    /// </summary>
    /// <param name="source">Source berry pool.</param>
    /// <param name="choose">Number of berries per combination.</param>
    /// <param name="action">
    /// Callback invoked for each generated combination.
    /// The span is only valid for the duration of the call.
    /// </param>
    public static void ForEach(
        ReadOnlySpan<BerryId> source,
        int choose,
        Action<ReadOnlySpan<BerryId>> action)
    {
        if (choose < 1 || choose > 4)
            throw new ArgumentOutOfRangeException(nameof(choose));

        switch (choose)
        {
            case 1:
                ForEach1(source, action);
                break;
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

    // ----------- N = 1 -----------

    private static void ForEach1(
        ReadOnlySpan<BerryId> s,
        Action<ReadOnlySpan<BerryId>> action)
    {
        Span<BerryId> buffer = stackalloc BerryId[1];

        for (int i = 0; i < s.Length; i++)
        {
            buffer[0] = s[i];
            action(buffer);
        }
    }

    // ----------- N = 2 -----------

    private static void ForEach2(
        ReadOnlySpan<BerryId> s,
        Action<ReadOnlySpan<BerryId>> action)
    {
        Span<BerryId> buffer = stackalloc BerryId[2];

        for (int i = 0; i < s.Length - 1; i++)
        {
            buffer[0] = s[i];

            for (int j = i + 1; j < s.Length; j++)
            {
                buffer[1] = s[j];
                action(buffer);
            }
        }
    }

    // ----------- N = 3 -----------

    private static void ForEach3(
        ReadOnlySpan<BerryId> s,
        Action<ReadOnlySpan<BerryId>> action)
    {
        Span<BerryId> buffer = stackalloc BerryId[3];

        for (int i = 0; i < s.Length - 2; i++)
        {
            buffer[0] = s[i];

            for (int j = i + 1; j < s.Length - 1; j++)
            {
                buffer[1] = s[j];

                for (int k = j + 1; k < s.Length; k++)
                {
                    buffer[2] = s[k];
                    action(buffer);
                }
            }
        }
    }

    // ----------- N = 4 -----------

    private static void ForEach4(
        ReadOnlySpan<BerryId> s,
        Action<ReadOnlySpan<BerryId>> action)
    {
        Span<BerryId> buffer = stackalloc BerryId[4];

        for (int i = 0; i < s.Length - 3; i++)
        {
            buffer[0] = s[i];

            for (int j = i + 1; j < s.Length - 2; j++)
            {
                buffer[1] = s[j];

                for (int k = j + 1; k < s.Length - 1; k++)
                {
                    buffer[2] = s[k];

                    for (int l = k + 1; l < s.Length; l++)
                    {
                        buffer[3] = s[l];
                        action(buffer);
                    }
                }
            }
        }
    }
}
