using ServicesBoard.Data;

namespace ServicesBoard;

using System.Diagnostics;


public partial class App : Application
{
    public static AppDatabase Database { get; private set; } = null!;
    public static Models.User? CurrentUser { get; private set; }

    public App(AppDatabase db)
    {
        InitializeComponent();
        Debug.WriteLine("DB PATH = " + DbConstants.DatabasePath);
        Database = db;
        MainPage = new AppShell();
    }

    public static void SetCurrentUser(Models.User? user) => CurrentUser = user;




}
