namespace ServicesBoard.Constants;

public static class AppLists
{
    public static readonly List<string> Areas = new()
    {
        "Центральный",
        "Ворошиловский",
        "Кировский",
        "Красноармейский",
        "Краснооктябрьский",
        "Дзержинский",
        "Тракторозаводский"
    };

    public static readonly List<string> Categories = new()
    {
        "Ремонт",
        "Уборка",
        "Доставка",
        "Красота",
        "Обучение",
        "IT/Компьютеры",
        "Другое"
    };

    public static readonly Dictionary<string, List<string>> CategoryImages = new()
    {
        ["Ремонт"] = new() { "fixer.jpg" },
        ["Уборка"] = new() { "clean.jpg" },
        ["Доставка"] = new() { "delivery.jpg" },
        ["Красота"] = new() { "beauty.jpg" },
        ["Обучение"] = new() { "teacher.jpg" },
        ["IT/Компьютеры"] = new() { "it.jpg" },
        ["Другое"] = new() { "other.jpg"}
    };
}
