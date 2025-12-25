using SQLite;

namespace ServicesBoard.Models;

[Table("Ads")]
public class ServiceAd
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Владелец объявления (исполнитель)
    [Indexed]
    public int UserId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double Price { get; set; }

    // Район/город
    public string Area { get; set; } = string.Empty;

    // Путь к фото или ссылка (опционально)
    public string PhotoPath { get; set; } = string.Empty;

    // "active" / "archived"
    public string Status { get; set; } = "active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ViewsCount { get; set; } = 0;
}
