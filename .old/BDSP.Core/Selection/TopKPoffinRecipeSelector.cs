using BDSP.Core.Berries.Data;
using BDSP.Core.Poffins;

namespace BDSP.Core.Selection;

public sealed class TopKPoffinRecipeSelector
{
    private readonly PoffinRecipeMinHeap _heap;
    private readonly IPoffinComparer _cmp;

    public TopKPoffinRecipeSelector(int k, IPoffinComparer comparer)
    {
        _heap = new PoffinRecipeMinHeap(k, comparer);
        _cmp = comparer;
    }

    public void Consider(in PoffinRecipe recipe)
    {
        if (_heap.Count < _heap.Capacity)
        {
            _heap.Add(recipe);
            return;
        }

        ref readonly var worst = ref _heap.GetRoot();
        if (_cmp.IsBetter(recipe.Poffin, worst.Poffin))
        {
            _heap.ReplaceRoot(recipe);
        }
    }

    public void Consider(in Poffin poffin, ReadOnlySpan<BerryId> berries)
    {
        if (_heap.Count < _heap.Capacity)
        {
            _heap.Add(new PoffinRecipe(poffin, berries.ToArray()));
            return;
        }

        ref readonly var worst = ref _heap.GetRoot();
        if (_cmp.IsBetter(poffin, worst.Poffin))
        {
            _heap.ReplaceRoot(new PoffinRecipe(poffin, berries.ToArray()));
        }
    }

    public ReadOnlySpan<PoffinRecipe> Results
        => _heap.AsSpan();
}
