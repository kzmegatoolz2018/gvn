<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Инструкции для GitHub Copilot

Этот проект представляет собой WPF приложение на C# для управления пользователями с использованием PostgreSQL базы данных.

## Архитектурные принципы

- **MVVM Pattern**: Используется паттерн Model-View-ViewModel
- **Repository Pattern**: Для абстракции работы с данными
- **Command Pattern**: Для обработки пользовательских действий
- **SOLID Principles**: Соблюдение всех принципов SOLID
- **Dependency Injection**: Простая реализация DI в App.xaml.cs

## Соглашения по коду

- Следовать Microsoft coding conventions для C#
- Использовать async/await для всех операций с базой данных
- Все публичные методы и классы должны иметь XML документацию
- Обработка исключений через try-catch с информативными сообщениями
- Использовать INotifyPropertyChanged для привязки данных
- Команды должны наследоваться от RelayCommand

## Структура проекта

- `Models/` - модели данных с INotifyPropertyChanged
- `ViewModels/` - ViewModels, наследующие от BaseViewModel
- `Views/` - XAML окна и UserControls
- `Repositories/` - интерфейсы и реализации репозиториев
- `Services/` - сервисы для бизнес-логики
- `Commands/` - реализации ICommand
- `Converters/` - конвертеры для привязки данных

## База данных

- PostgreSQL с таблицами users и roles
- Использование Npgsql для подключения
- Все запросы должны быть параметризованными
- Обработка null значений из БД

## Примеры кода

При создании нового ViewModel:
```csharp
public class NewViewModel : BaseViewModel
{
    private bool _isLoading;
    
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
}
```

При создании команды:
```csharp
public ICommand MyCommand { get; }

// В конструкторе:
MyCommand = new RelayCommand(async () => await ExecuteMyCommandAsync(), CanExecuteMyCommand);
```

При работе с репозиторием:
```csharp
public async Task<IEnumerable<User>> GetUsersAsync()
{
    try
    {
        // Код работы с БД
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Ошибка при получении данных: {ex.Message}", ex);
    }
}
```
