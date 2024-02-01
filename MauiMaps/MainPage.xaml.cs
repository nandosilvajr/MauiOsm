using Mapsui.ViewModels;

namespace MauiMaps;

public partial class MainPage : ContentPage
{
    MainViewModel _viewModel => BindingContext as MainViewModel;

    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
    
}


