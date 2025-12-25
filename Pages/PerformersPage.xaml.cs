using ServicesBoard.Data;

namespace ServicesBoard.Pages;

public partial class PerformersPage : ContentPage
{
    private readonly AppDatabase _db;

    public PerformersPage()
    {
        InitializeComponent();
        _db = App.Database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPerformersAsync();
    }

    private async Task LoadPerformersAsync()
    {
        var performers = await _db.GetPerformersAsync();
        PerformersCollectionView.ItemsSource = performers;
    }
}
