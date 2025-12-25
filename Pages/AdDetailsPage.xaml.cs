using ServicesBoard.Data;
using ServicesBoard.Models;

namespace ServicesBoard.Pages;

[QueryProperty(nameof(AdId), "adId")]
public partial class AdDetailsPage : ContentPage
{
    private readonly AppDatabase _db;
    private ServiceAd? _ad;
    private User? _performer;

    public int AdId { get; set; }

    public AdDetailsPage()
    {
        InitializeComponent();
        _db = App.Database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _ad = await _db.GetAdByIdAsync(AdId);
        if (_ad == null)
            return;

        // +1 просмотр (только если не владелец и не админ — можно менять по желанию)
        if (App.CurrentUser == null || (App.CurrentUser.Id != _ad.UserId && App.CurrentUser.Role != "admin"))
        {
            await _db.IncrementAdViewsAsync(_ad.Id);
            _ad = await _db.GetAdByIdAsync(AdId);
        }

        // Заполняем объявление
        TitleLabel.Text = _ad.Title;
        MetaLabel.Text = $"{_ad.Category} • {_ad.Area} • {_ad.CreatedAt:dd.MM.yyyy HH:mm}";
        ViewsLabel.Text = $"Просмотров: {_ad.ViewsCount}";

        CategoryLabel.Text = _ad.Category;
        AreaLabel.Text = _ad.Area;
        PriceLabel.Text = $"{_ad.Price:F2} ₽";
        StatusLabel.Text = _ad.Status == "active" ? "Активно" : "В архиве";
        DescLabel.Text = _ad.Description;

        if (!string.IsNullOrWhiteSpace(_ad.PhotoPath))
        {
            AdImage.IsVisible = true;
            NoPhotoBlock.IsVisible = false;
            AdImage.Source = ImageSource.FromFile(_ad.PhotoPath);
        }
        else
        {
            AdImage.IsVisible = false;
            NoPhotoBlock.IsVisible = true;
        }


        // Исполнитель
        _performer = await _db.GetUserByIdAsync(_ad.UserId);
        if (_performer != null)
        {
            PerformerNameLabel.Text = _performer.Name;
            PerformerDescLabel.Text = string.IsNullOrWhiteSpace(_performer.Description)
                ? "Описание не указано"
                : _performer.Description;

            PerformerPhoneLabel.Text = _performer.Phone;
            PerformerEmailLabel.Text = _performer.Email;
            PerformerRatingLabel.Text = $"{_performer.Rating:F1} (отзывов: {_performer.ReviewsCount})";
            PerformerHoursLabel.Text = string.IsNullOrWhiteSpace(_performer.WorkingHours) ? "Не указано" : _performer.WorkingHours;
            PerformerOrdersLabel.Text = $"Запросов на услуги: {_performer.OrdersCount}";
        }

        // Кнопка "Заказать": видимость/доступ
        if (App.CurrentUser == null)
        {
            RequestButton.IsEnabled = true; // можно нажать — попросит войти
        }
        else
        {
            // Заказывать может только заказчик (и можно разрешить админу при желании — но по твоим правилам админ не создаёт объявления,
            // а заказывать может/не может — решай. Сейчас сделаем как заказчик.
            RequestButton.IsEnabled = true;
        }
    }

    private async void OnRequestClicked(object sender, EventArgs e)
    {
        if (_ad == null)
            return;

        if (App.CurrentUser == null)
        {
            await DisplayAlert("Вход", "Для отправки запроса нужно войти.", "OK");
            return;
        }

        // Только заказчик может заказывать
        if (App.CurrentUser.Role != "customer")
        {
            await DisplayAlert("Роль", "Только заказчик может отправлять запросы.", "OK");
            return;
        }

        if (_ad.UserId == App.CurrentUser.Id)
        {
            await DisplayAlert("Ошибка", "Нельзя заказать услугу у самого себя.", "OK");
            return;
        }

        var request = new Request
        {
            AdId = _ad.Id,
            CustomerId = App.CurrentUser.Id,
            PerformerId = _ad.UserId,
            Message = $"Хочу заказать услугу: {_ad.Title}",
            Status = "new",
            CreatedAt = DateTime.Now
        };

        await _db.SaveRequestAsync(request);
        await _db.IncrementPerformerOrdersCountAsync(_ad.UserId);

        await DisplayAlert("Запрос отправлен", "Исполнителю отправлено уведомление (заглушка).", "OK");
    }
}
