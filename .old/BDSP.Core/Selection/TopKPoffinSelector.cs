using BDSP.Core.Poffins;

namespace BDSP.Core.Selection;

/// <summary>
/// Maintains the top-K best Poffins using a bounded min-heap.
/// </summary>
public sealed class TopKPoffinSelector
{
    private readonly PoffinMinHeap _heap;
    private readonly IPoffinComparer _cmp;

    public TopKPoffinSelector(int k, IPoffinComparer comparer)
    {
        _heap = new PoffinMinHeap(k, comparer);
        _cmp = comparer;
    }
    /// <summary>
    /// Considers a Poffin for inclusion in the top-K set.
    /// </summary>
    public void Consider(in Poffin p)
    {
        // Always accept until heap is full
        if (_heap.Count < _heap.Capacity)
        {
            _heap.Add(p);
            return;
        }

        // Root is the WORST of the kept poffins
        ref readonly var worst = ref _heap.GetRoot();

        // Replace only if candidate is strictly better
        if (_cmp.IsBetter(p, worst))
        {
            _heap.ReplaceRoot(p);
        }
    }

    /// <summary>
    /// Returns the current best Poffins.
    /// </summary>
    public ReadOnlySpan<Poffin> Results
        => _heap.AsSpan();
}
