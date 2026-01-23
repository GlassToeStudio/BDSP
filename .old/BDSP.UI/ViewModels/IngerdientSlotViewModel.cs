
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace BDSP.UI.ViewModels;

public sealed class IngredientSlotViewModel : INotifyPropertyChanged
{
    public int Index { get; }

    private BerryViewModel? _berry;
    public BerryViewModel? Berry
    {
        get => _berry;
        set
        {
            if (_berry == value)
                return;

            _berry = value;
            OnPropertyChanged();
        }
    }

    public IngredientSlotViewModel(int index)
    {
        Index = index;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
