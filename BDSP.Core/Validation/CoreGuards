using System.Diagnostics;

namespace BDSP.Core.Validation;

/// <summary>
/// Debug-only invariant checks for BDSP.Core.
/// </summary>
internal static class CoreGuards
{
    [Conditional("DEBUG")]
    public static void AssertNonNegative(byte value, string name)
    {
        Debug.Assert(value >= 0, $"{name} must be non-negative.");
    }

    [Conditional("DEBUG")]
    public static void AssertValidFlavorTotal(int value)
    {
        Debug.Assert(value >= -255 && value <= 255);
    }
}
