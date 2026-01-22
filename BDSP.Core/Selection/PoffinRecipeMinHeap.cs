using System.Runtime.CompilerServices;
using BDSP.Core.Poffins;

namespace BDSP.Core.Selection;

internal sealed class PoffinRecipeMinHeap
{
    private readonly PoffinRecipe[] _data;
    private readonly IPoffinComparer _cmp;
    private int _count;

    public int Count => _count;
    public int Capacity => _data.Length;

    public PoffinRecipeMinHeap(int capacity, IPoffinComparer comparer)
    {
        _data = new PoffinRecipe[capacity];
        _cmp = comparer;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref PoffinRecipe GetRoot()
    {
        return ref _data[0];
    }

    public void Add(in PoffinRecipe r)
    {
        _data[_count] = r;
        HeapifyUp(_count++);
    }

    public void ReplaceRoot(in PoffinRecipe r)
    {
        _data[0] = r;
        HeapifyDown(0);
    }

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) >> 1;
            if (_cmp.IsBetter(_data[parent].Poffin, _data[i].Poffin))
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

            if (right < _count &&
                _cmp.IsBetter(_data[left].Poffin, _data[right].Poffin))
            {
                worst = right;
            }

            if (!_cmp.IsBetter(_data[i].Poffin, _data[worst].Poffin))
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

    public ReadOnlySpan<PoffinRecipe> AsSpan()
        => _data.AsSpan(0, _count);
}
