using System.IO;
using SQLite;
using Microsoft.Maui.Storage;

namespace ServicesBoard;

public static class DbConstants
{
    public const string DatabaseFilename = "ServicesBoard.db3";

    // Флаги открытия БД (чтение/запись, создать при отсутствии, общий кэш)
    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    // Полный путь к файлу БД в папке приложения
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}
