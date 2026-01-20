using System.Runtime.CompilerServices;

namespace BDSP.Core.Berries;

public static class BerryExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this BerryId id)
        => BerryNames.GetName(id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this Berry berry)
        => BerryNames.GetName(berry.Id);
}
