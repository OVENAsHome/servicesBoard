using ServicesBoard.Models;

namespace ServicesBoard.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly Data.AppDatabase _db;

    public RegisterPage()
    {
        InitializeComponent();
        _db = App.Database;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Ошибка", "Заполните имя, email и пароль", "OK");
            return;
        }

        if (RolePicker.SelectedItem == null)
        {
            await DisplayAlert("Ошибка", "Выберите роль", "OK");
            return;
        }

        var existing = await _db.GetUserByEmailAsync(EmailEntry.Text.Trim());
        if (existing != null)
        {
            await DisplayAlert("Ошибка", "Пользователь с таким email уже зарегистрирован", "OK");
            return;
        }

        string selectedRole = RolePicker.SelectedItem.ToString() ?? "Заказчик";
        string role;

        if (selectedRole == "Исполнитель")
        {
            role = "worker";

            if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
            {
                await DisplayAlert("Ошибка", "Для исполнителя обязательно заполнить описание.", "OK");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(WorkingHoursEntry.Text))
            {
                await DisplayAlert("Ошибка", "Для исполнителя обязательно указать время работы.", "OK");
                return;
            }

        }
        else if (selectedRole == "Администратор")
        {
            if (AdminCodeEntry.Text?.Trim() != "2005")
            {
                await DisplayAlert("Ошибка", "Неверный код администратора. Введите 2005.", "OK");
                return;
            }

            role = "admin";
        }
        else
        {
            role = "customer";
        }

        var user = new User
        {
            Name = NameEntry.Text.Trim(),
            Phone = PhoneEntry.Text?.Trim() ?? "",
            Email = EmailEntry.Text.Trim(),
            Password = PasswordEntry.Text.Trim(),
            Role = role,
            Description = DescriptionEditor.Text?.Trim() ?? "",
            WorkingHours = WorkingHoursEntry.Text?.Trim() ?? ""

        };

        await _db.SaveUserAsync(user);

        App.SetCurrentUser(user);

        await DisplayAlert("Успех", $"Регистрация прошла успешно. Ваша роль: {role}", "OK");

        await Shell.Current.GoToAsync("..");
    }
}
