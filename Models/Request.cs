using SQLite;

namespace ServicesBoard.Models;

[Table("Requests")]
public class Request
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int AdId { get; set; }

    [Indexed]
    public int CustomerId { get; set; }

    [Indexed]
    public int PerformerId { get; set; }

    public string Message { get; set; } = string.Empty;

    // new / accepted / rejected / done
    public string Status { get; set; } = "new";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
