using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Linq;
using BDSP.Core.Berries;
using BDSP.Core.Poffins;
using BDSP.Core.Runner;
using BDSP.Core.Selection;
using BDSP.Criteria;
using BDSP.UI.Commands;
using BDSP.UI.Mapping;
using BDSP.UI.Models;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;



namespace BDSP.UI.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<IngredientSlotViewModel> Ingredients { get; }
        = new()
        {
        new IngredientSlotViewModel(0),
        new IngredientSlotViewModel(1),
        new IngredientSlotViewModel(2),
        new IngredientSlotViewModel(3)
        };

    public RelayCommand CookPoffinCommand { get; }

    #region Berry List and Filter

    private readonly List<BerryViewModel> _allBerries = new(BerryTable.All.Length);

    private string _berrySearchText = string.Empty;
    public string BerrySearchText
    {
        get => _berrySearchText;
        set
        {
            if (!SetField(ref _berrySearchText, value))
                return;

            ApplyBerryFilter();
        }
    }


    private IReadOnlyList<BerryViewModel> _filteredBerries;
    public IReadOnlyList<BerryViewModel> FilteredBerries
    {
        get => _filteredBerries;
        private set => SetField(ref _filteredBerries, value);
    }

    #endregion


    #region Selected Berry and Radar
    public bool HasSelectedBerry => SelectedBerryInfo != null;

    private BerryViewModel? _selectedBerryInfo;
    public BerryViewModel? SelectedBerryInfo
    {
        get => _selectedBerryInfo;
        set
        {
            SetField(ref _selectedBerryInfo, value);
            OnPropertyChanged(nameof(HasSelectedBerry));
        }
    }

    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private Berry? _selectedBerry;
    public Berry? SelectedBerry
    {
        get => _selectedBerry;
        set
        {
            SetField(ref _selectedBerry, value);
            

            //_selectedBerry = value;
            Debug.WriteLine($"SelectedBerry set: {value?.Id.Value}");

            SelectedBerryRadar =
                value.HasValue
                    ? BerryRadarMapping.FromBerry(value.Value)
                    : null;

            SelectedBerryInfo = new BerryViewModel((Berry)value);
        }
    }

    private RadarData? _selectedBerryRadar;
    public RadarData? SelectedBerryRadar
    {
        get => _selectedBerryRadar;
        private set => SetField(ref _selectedBerryRadar, value);
    }

    #endregion


    #region Poffin

    private ContestRadarData? _resultPoffinRadar;
    public ContestRadarData? ResultPoffinRadar
    {
        get => _resultPoffinRadar;
        private set
        {
            SetField(ref _resultPoffinRadar, value);
        }
    }


    #endregion

    public MainViewModel()
    {
        CookPoffinCommand = new RelayCommand(CookPoffin);

        //_allBerries = [.. BerryTable.All.ToImmutableArray().Select((Berry b) => new BerryViewModel(b))];
        foreach (var b in BerryTable.All)
        {
            _allBerries.Add(new BerryViewModel(b));
        }
        _filteredBerries = _allBerries;
    }

    private void ApplyBerryFilter()
    {
        if (string.IsNullOrWhiteSpace(BerrySearchText))
        {
            FilteredBerries = _allBerries;
            return;
        }

        var text = BerrySearchText.Trim().ToLowerInvariant();

        FilteredBerries = [.. _allBerries.Where(b => b.Name.ToLowerInvariant().Contains(text))];
    }


    public void SetIngredient(int index, BerryViewModel berry)
    {
        if (index < 0 || index >= Ingredients.Count)
            return;

        Ingredients[index].Berry = berry;
        CookPoffin();
    }

    public void ClearIngredient(int index)
    {
        if (index < 0 || index >= Ingredients.Count)
            return;

        Ingredients[index] = null;
    }

    private void CookPoffin()
    {
        // Collect selected berries
        var berries = Ingredients
            .Where(s => s.Berry != null)
            .Select(s => s.Berry!.Berry.Id)
            .ToArray();

        if (berries.Length == 0)
        {
            ResultPoffinRadar = null;
            return;
        }

        // Build poffin (adjust parameters if you expose them later)
        var poffin = PoffinCooker.Cook(
            berries,
            cookTimeSeconds: 40,
            errors: 0,
            amityBonus: 9);

        // Map poffin → radar
        ResultPoffinRadar = RadarMapping.FromPoffin(poffin);
    }

    #region ---- INotifyPropertyChanged ----
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
    #endregion
}
