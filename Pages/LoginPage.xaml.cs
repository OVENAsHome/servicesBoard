namespace ServicesBoard.Pages;

public partial class LoginPage : ContentPage
{
    private readonly Data.AppDatabase _db;

    public LoginPage()
    {
        InitializeComponent();
        _db = App.Database;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Ошибка", "Введите email и пароль", "OK");
            return;
        }

        var user = await _db.GetUserByEmailAndPasswordAsync(email, password);
        if (user == null)
        {
            await DisplayAlert("Ошибка", "Неверный email или пароль", "OK");
            return;
        }

        App.SetCurrentUser(user);

        await DisplayAlert("Успех", $"Вы вошли как {user.Name} ({user.Role})", "OK");

        await Shell.Current.GoToAsync("..");
    }
}
