using SQLite;

namespace ServicesBoard.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    [NotNull, Unique]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    // "customer" (заказчик), "worker" (исполнитель), "admin" (администратор)
    [NotNull]
    public string Role { get; set; } = "customer";

    // Описание исполнителя (резюме)
    public string Description { get; set; } = string.Empty;

    // Средний рейтинг исполнителя
    public double Rating { get; set; } = 0;

    // Количество отзывов
    public int ReviewsCount { get; set; } = 0;

    // Сколько раз по объявлениям этого исполнителя нажали "Заказать"
    public int OrdersCount { get; set; } = 0;
    public string WorkingHours { get; set; } = "";

}
