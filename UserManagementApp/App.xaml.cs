using System;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using UserManagementApp.Repositories;
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

            // Настройка Dependency Injection (упрощенная реализация)
            var tableRepository = new TableRepository(connectionString);
            var tableViewModel = new TableViewModel(tableRepository);
            var mainWindow = new MainWindow(tableViewModel);

            mainWindow.Show();
        }
        catch (System.Windows.Markup.XamlParseException xamlEx)
        {
            var errorMessage = $"Ошибка парсинга XAML (WPF): {xamlEx.Message}\n\n" +
                             $"Строка: {xamlEx.LineNumber}\n" +
                             $"Позиция: {xamlEx.LinePosition}\n" +
                             $"Источник: {xamlEx.Source}\n\n" +
                             $"Внутренняя ошибка: {xamlEx.InnerException?.Message}\n\n" +
                             $"Полная трассировка стека:\n{xamlEx}";
            
            MessageBox.Show(errorMessage, "Ошибка XAML (WPF)", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
        catch (System.Xaml.XamlParseException xamlEx)
        {
            var errorMessage = $"Ошибка парсинга XAML (System.Xaml): {xamlEx.Message}\n\n" +
                             $"Строка: {xamlEx.LineNumber}\n" +
                             $"Позиция: {xamlEx.LinePosition}\n\n" +
                             $"Внутренняя ошибка: {xamlEx.InnerException?.Message}\n\n" +
                             $"Полная трассировка стека:\n{xamlEx}";
            
            MessageBox.Show(errorMessage, "Ошибка XAML (System.Xaml)", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Критическая ошибка при запуске приложения:\n\n" +
                             $"Тип ошибки: {ex.GetType().Name}\n" +
                             $"Сообщение: {ex.Message}\n\n";

            if (ex.InnerException != null)
            {
                errorMessage += $"Внутренняя ошибка: {ex.InnerException.GetType().Name}\n" +
                              $"Сообщение внутренней ошибки: {ex.InnerException.Message}\n\n";
            }

            errorMessage += $"Полная трассировка стека:\n{ex}";
            
            MessageBox.Show(errorMessage, "Критическая ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}

