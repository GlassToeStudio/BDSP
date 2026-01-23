using System.Runtime.CompilerServices;
using BDSP.Core.Poffins;

namespace BDSP.Core.Selection;

internal sealed class PoffinMinHeap
{
    private readonly Poffin[] _data;
    private readonly IPoffinComparer _cmp;
    private int _count;

    public int Count => _count;
    public int Capacity => _data.Length;

    public PoffinMinHeap(int capacity, IPoffinComparer comparer)
    {
        _data = new Poffin[capacity];
        _cmp = comparer;
        _count = 0;
    }

    // ✅ METHOD, not property
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Poffin GetRoot()
    {
        return ref _data[0];
    }

    public void Add(in Poffin p)
    {
        _data[_count] = p;
        HeapifyUp(_count++);
    }

    public void ReplaceRoot(in Poffin p)
    {
        _data[0] = p;
        HeapifyDown(0);
    }

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) >> 1;

            if (_cmp.IsBetter(_data[parent], _data[i]))
                break;

            Swap(i, parent);
            i = parent;
        }
    }

    private void HeapifyDown(int i)
    {
        while (true)
        {
            int left = (i << 1) + 1;
            if (left >= _count)
                return;

            int right = left + 1;
            int worst = left;

            // Pick the WORSE child
            if (right < _count &&
                _cmp.IsBetter(_data[left], _data[right]))
            {
                worst = right;
            }

            // If parent is already worse or equal, stop
            if (!_cmp.IsBetter(_data[i], _data[worst]))
                return;

            Swap(i, worst);
            i = worst;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Swap(int a, int b)
    {
        var tmp = _data[a];
        _data[a] = _data[b];
        _data[b] = tmp;
    }

    public ReadOnlySpan<Poffin> AsSpan()
        => _data.AsSpan(0, _count);
}
