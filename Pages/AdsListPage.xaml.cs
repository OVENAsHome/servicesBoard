using ServicesBoard.Data;
using ServicesBoard.Models;
using ServicesBoard.Constants;

namespace ServicesBoard.Pages;

public partial class AdsListPage : ContentPage
{
    private readonly AppDatabase _db;

    public AdsListPage()
    {
        InitializeComponent();

        _db = App.Database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        CategoryFilterPicker.ItemsSource = AppLists.Categories;
        AreaFilterPicker.ItemsSource = AppLists.Areas;

        await LoadAdsAsync();
        UpdateLoginButtonText();
    }


    private async Task LoadAdsAsync()
    {
        var ads = await _db.GetActiveAdsAsync();
        AdsCollectionView.ItemsSource = ads;
    }

    private void UpdateLoginButtonText()
    {
        if (App.CurrentUser == null)
            LoginButton.Text = "Войти";
        else
            LoginButton.Text = $"Выйти ({App.CurrentUser.Name})";
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        // Создавать объявления могут исполнитель и админ
        if (App.CurrentUser == null)
        {
            await DisplayAlert("Доступ запрещён", "Для размещения объявления нужно войти в систему.", "OK");
            return;
        }

        if (App.CurrentUser.Role != "worker")
        {
            await DisplayAlert("Доступ запрещён", "Только исполнитель может создавать объявления.", "OK");
            return;
        }


        await Shell.Current.GoToAsync(nameof(CreateAdPage));
    }

    private async void OnFilterClicked(object sender, EventArgs e)
    {
        string? category = CategoryFilterPicker.SelectedItem?.ToString();
        string? area = AreaFilterPicker.SelectedItem?.ToString();

        double? minPrice = double.TryParse(MinPriceEntry.Text, out var p1) ? p1 : null;
        double? maxPrice = double.TryParse(MaxPriceEntry.Text, out var p2) ? p2 : null;
        double? minRating = double.TryParse(MinRatingEntry.Text, out var r) ? r : null;

        var ads = await _db.GetFilteredAdsAsync(category, minPrice, maxPrice, area, minRating);
        AdsCollectionView.ItemsSource = ads;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (App.CurrentUser == null)
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
        else
        {
            App.SetCurrentUser(null);
            await DisplayAlert("Выход", "Вы вышли из аккаунта.", "OK");
            UpdateLoginButtonText();
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    // Заказ услуги заказчиком (и админом)
    private async void OnRequestClicked(object sender, EventArgs e)
    {
        if (App.CurrentUser == null)
        {
            await DisplayAlert("Вход", "Для отправки запроса нужно войти.", "OK");
            return;
        }

        // разрешаем customer и admin
        if (App.CurrentUser.Role != "customer" && App.CurrentUser.Role != "admin")
        {
            await DisplayAlert("Роль", "Только заказчики или администратор могут отправлять запрос на услугу.", "OK");
            return;
        }

        if (sender is Button button && button.BindingContext is ServiceAd ad)
        {
            if (ad.UserId == App.CurrentUser.Id)
            {
                await DisplayAlert("Ошибка", "Нельзя отправить запрос самому себе.", "OK");
                return;
            }

            var request = new Request
            {
                AdId = ad.Id,
                CustomerId = App.CurrentUser.Id,
                PerformerId = ad.UserId,
                Message = $"Хочу заказать услугу: {ad.Title}",
                Status = "new"
            };

            await _db.SaveRequestAsync(request);

            // увеличиваем счётчик заказов у исполнителя
            await _db.IncrementPerformerOrdersCountAsync(ad.UserId);

            await DisplayAlert("Запрос отправлен", "Исполнителю отправлено уведомление о вашем запросе (заглушка).", "OK");
        }
    }

    // Редактирование объявления
    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (App.CurrentUser == null)
        {
            await DisplayAlert("Ошибка", "Нужно войти в систему.", "OK");
            return;
        }

        if (sender is Button button && button.BindingContext is ServiceAd ad)
        {
            bool isAdmin = App.CurrentUser.Role == "admin";
            bool isOwner = App.CurrentUser.Id == ad.UserId;

            if (!isAdmin && !isOwner)
            {
                await DisplayAlert("Ошибка", "Вы не можете редактировать чужое объявление.", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(CreateAdPage)}?adId={ad.Id}");
        }
    }

    // Удаление объявления
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (App.CurrentUser == null)
        {
            await DisplayAlert("Ошибка", "Нужно войти в систему.", "OK");
            return;
        }

        if (sender is Button button && button.BindingContext is ServiceAd ad)
        {
            bool isAdmin = App.CurrentUser.Role == "admin";
            bool isOwner = App.CurrentUser.Id == ad.UserId;

            if (!isAdmin && !isOwner)
            {
                await DisplayAlert("Ошибка", "Вы не можете удалять чужое объявление.", "OK");
                return;
            }

            bool confirm = await DisplayAlert(
                "Подтверждение",
                "Вы действительно хотите удалить объявление?",
                "Да", "Нет");

            if (!confirm)
                return;

            await _db.DeleteAdAsync(ad);
            await LoadAdsAsync();
        }
    }

    private async void OnMyAdsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MyAdsPage));
    }

    private async void OnPerformersClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PerformersPage));
    }
    private async void OnAdSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ServiceAd ad)
        {
            AdsCollectionView.SelectedItem = null;
            await Shell.Current.GoToAsync($"{nameof(AdDetailsPage)}?adId={ad.Id}");
        }
    }

}
