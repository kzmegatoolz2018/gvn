using System;
using System.Windows;
using UserManagementApp.Repositories;
using UserManagementApp.Services;
using UserManagementApp.ViewModels;

namespace UserManagementApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string DefaultConnectionString = 
        "Host=26.12.33.238;Database=cmd_db2;Username=postgres;Password=1;Port=5432;";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Можно получить строку подключения из конфигурации или переменных среды
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                                 ?? DefaultConnectionString;

            // Настройка Dependency Injection (простая реализация)
            var userRepository = new UserRepository(connectionString);
            var roleRepository = new RoleRepository(connectionString);
            var jsonService = new JsonService();

            var mainViewModel = new MainViewModel(userRepository, roleRepository, jsonService);
            var mainWindow = new MainWindow(mainViewModel);

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}", "Критическая ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}

