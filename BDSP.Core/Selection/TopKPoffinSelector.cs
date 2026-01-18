using BDSP.Core.Poffins;

namespace BDSP.Core.Selection;

public sealed class TopKPoffinSelector
{
    private readonly PoffinMinHeap _heap;
    private readonly IPoffinComparer _cmp;

    public TopKPoffinSelector(int k, IPoffinComparer comparer)
    {
        _heap = new PoffinMinHeap(k, comparer);
        _cmp = comparer;
    }

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

    public ReadOnlySpan<Poffin> Results
        => _heap.AsSpan();
}
