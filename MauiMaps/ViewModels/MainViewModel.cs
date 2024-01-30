using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapsui.Logging;
using Mapsui.UI.Maui;

#pragma warning disable IDISP008 // Don't assign member with injected and created disposables.

namespace Mapsui.Samples.Maui.ViewModel;

public partial class MainViewModel : ObservableObject
{

    public MainViewModel()
    {
        Map = new Map();
    }

    [ObservableProperty]
    string selectedCategory;


    [ObservableProperty]
    Map? map;

    public ObservableCollection<string> Categories { get; } = new();

    // MapControl is needed in the samples. Mapsui's design should change so this is not needed anymore.
    public MapControl? MapControl { get; set; }
    
}
