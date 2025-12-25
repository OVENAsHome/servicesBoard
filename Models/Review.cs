using SQLite;

namespace ServicesBoard.Models;

[Table("Reviews")]
public class Review
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PerformerId { get; set; }

    [Indexed]
    public int CustomerId { get; set; }

    // Оценка 1–5
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
