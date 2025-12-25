using ServicesBoard.Data;
using ServicesBoard.Models;

namespace ServicesBoard.Pages;

public partial class MyAdsPage : ContentPage
{
    private readonly AppDatabase _db;

    public MyAdsPage()
    {
        InitializeComponent();
        _db = App.Database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (App.CurrentUser == null)
        {
            await DisplayAlert("Ошибка", "Нужно войти в систему.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        if (App.CurrentUser.Role != "worker" && App.CurrentUser.Role != "admin")
        {
            await DisplayAlert("Доступ", "Раздел 'Мои объявления' доступен только исполнителям и администратору.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        await LoadMyAdsAsync();
    }

    private async Task LoadMyAdsAsync()
    {
        var ads = await _db.GetAdsByUserAsync(App.CurrentUser!.Id);
        MyAdsCollectionView.ItemsSource = ads;
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ServiceAd ad)
        {
            await Shell.Current.GoToAsync($"{nameof(CreateAdPage)}?adId={ad.Id}");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ServiceAd ad)
        {
            bool confirm = await DisplayAlert("Удаление", "Удалить объявление?", "Да", "Нет");
            if (!confirm)
                return;

            await _db.DeleteAdAsync(ad);
            await LoadMyAdsAsync();
        }
    }

    private async void OnToggleStatusClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ServiceAd ad)
        {
            ad.Status = ad.Status == "active" ? "archived" : "active";
            await _db.SaveAdAsync(ad);
            await LoadMyAdsAsync();
        }
    }
}
