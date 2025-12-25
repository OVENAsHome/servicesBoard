using Microsoft.Maui.Controls;
using ServicesBoard.Models;
using ServicesBoard.Constants;

namespace ServicesBoard.Pages;

[QueryProperty(nameof(AdId), "adId")]
public partial class CreateAdPage : ContentPage
{
    private readonly Data.AppDatabase _db;

    private ServiceAd? _currentAd;

    private int _adId;
    public int AdId
    {
        get => _adId;
        set
        {
            _adId = value;
            if (_adId > 0)
            {
                // загрузить объявление для редактирования
                _ = LoadAdAsync(_adId);
            }
        }
    }

    public CreateAdPage()
    {
        InitializeComponent();
        _db = App.Database;
        CategoryPicker.ItemsSource = AppLists.Categories;
        AreaPicker.ItemsSource = AppLists.Areas;

        CategoryPicker.SelectedIndexChanged += (_, __) => RefreshCategoryImages();
        CategoryImagePicker.SelectedIndexChanged += (_, __) => OnCategoryImageSelected();

    }

    private void RefreshCategoryImages()
    {
        var category = CategoryPicker.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(category)) return;

        if (AppLists.CategoryImages.TryGetValue(category, out var imgs))
            CategoryImagePicker.ItemsSource = imgs;
        else
            CategoryImagePicker.ItemsSource = new List<string>();

        CategoryImagePicker.SelectedItem = null;
    }

    private void OnCategoryImageSelected()
    {
        var img = CategoryImagePicker.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(img)) return;

        // Сохраняем путь как имя ресурса
        PhotoPath = img;
        PreviewImage.Source = ImageSource.FromFile(img);
    }

    // будем хранить выбранный путь (либо ресурс, либо файл)
    private string PhotoPath = "";


    private async void OnPickFromGalleryClicked(object sender, EventArgs e)
    {
        try
        {
            var file = await MediaPicker.Default.PickPhotoAsync();
            if (file == null) return;

            // копируем в локальную папку приложения
            var newFile = Path.Combine(FileSystem.AppDataDirectory, file.FileName);

            await using var stream = await file.OpenReadAsync();
            await using var newStream = File.OpenWrite(newFile);
            await stream.CopyToAsync(newStream);

            PhotoPath = newFile;
            PreviewImage.Source = ImageSource.FromFile(newFile);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
        }
    }


    private async Task LoadAdAsync(int id)
    {
        _currentAd = await _db.GetAdByIdAsync(id);
        if (_currentAd == null)
            return;

        TitleEntry.Text = _currentAd.Title;
        DescriptionEditor.Text = _currentAd.Description;
        PriceEntry.Text = _currentAd.Price.ToString();
        AreaPicker.SelectedItem = _currentAd.Area;
        PhotoPath = _currentAd.PhotoPath ?? "";
        if (!string.IsNullOrWhiteSpace(PhotoPath))
            PreviewImage.Source = ImageSource.FromFile(PhotoPath);


        // Категория – выбираем в Picker, если есть
        if (!string.IsNullOrWhiteSpace(_currentAd.Category))
        {
            for (int i = 0; i < CategoryPicker.Items.Count; i++)
            {
                if (CategoryPicker.Items[i] == _currentAd.Category)
                {
                    CategoryPicker.SelectedIndex = i;
                    break;
                }
            }
        }

        Title = "Редактирование объявления";
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (App.CurrentUser == null)
        {
            await DisplayAlert("Ошибка", "Нужно войти в систему.", "OK");
            return;
        }

        // Для создания: только исполнитель или админ
        bool isWorker = App.CurrentUser.Role == "worker";
        bool isAdmin = App.CurrentUser.Role == "admin";

        if (_currentAd == null)
        {
            if (!isWorker)
            {
                await DisplayAlert("Ошибка", "Только исполнитель может создавать объявления.", "OK");
                return;
            }
        }
        else
        {
            // редактировать может владелец или админ
            if (!isAdmin && _currentAd.UserId != App.CurrentUser.Id)
            {
                await DisplayAlert("Ошибка", "Вы не можете редактировать чужое объявление.", "OK");
                return;
            }
        }


        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            await DisplayAlert("Ошибка", "Введите название объявления", "OK");
            return;
        }

        if (CategoryPicker.SelectedItem is null)
        {
            await DisplayAlert("Ошибка", "Выберите категорию", "OK");
            return;
        }

        if (!double.TryParse(PriceEntry.Text, out var price))
        {
            await DisplayAlert("Ошибка", "Введите корректную цену", "OK");
            return;
        }

        var category = CategoryPicker.SelectedItem.ToString() ?? "";
        var description = DescriptionEditor.Text?.Trim() ?? "";
        var area = AreaPicker.SelectedItem?.ToString() ?? "";
        var photo = PhotoPath;

        if (_currentAd == null)
        {
            // Создание нового объявления
            var ad = new ServiceAd
            {
                Title = TitleEntry.Text!.Trim(),
                Category = category,
                Description = description,
                Price = price,
                Area = area,
                PhotoPath = photo,
                UserId = App.CurrentUser.Id,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            await _db.SaveAdAsync(ad);
        }
        else
        {
            // Редактирование существующего
            _currentAd.Title = TitleEntry.Text!.Trim();
            _currentAd.Category = category;
            _currentAd.Description = description;
            _currentAd.Price = price;
            _currentAd.Area = area;
            _currentAd.PhotoPath = photo;

            await _db.SaveAdAsync(_currentAd);
        }

        await DisplayAlert("Готово", "Объявление сохранено", "OK");

        await Shell.Current.GoToAsync("..");
    }
}
