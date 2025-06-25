## Возможности

- **Просмотр пользователей**: Отображение списка всех пользователей с их ролями
- **Добавление пользователей**: Создание новых пользователей с выбором роли
- **Удаление пользователей**: Удаление выбранного пользователя
- **Экспорт в JSON**: Выгрузка списка пользователей в JSON файл
- **Импорт из JSON**: Загрузка пользователей из JSON файла

## Структура проекта

```
UserManagementApp/
├── Models/                    # Модели данных
│   ├── User.cs                # Модель пользователя
│   ├── Role.cs                # Модель роли
│   └── UserExportModel.cs     # Модель для экспорта/импорта JSON
├── ViewModels/                # ViewModels
│   ├── BaseViewModel.cs       # Базовый ViewModel
│   ├── MainViewModel.cs       # Главный ViewModel
│   └── AddUserViewModel.cs    # ViewModel для добавления пользователя
├── Views/                     # Представления (окна)
│   └── AddUserWindow.xaml     # Окно добавления пользователя
├── Repositories/              # Репозитории для работы с данными
│   ├── IUserRepository.cs     # Интерфейс пользовательского репозитория
│   ├── UserRepository.cs      # Реализация пользовательского репозитория
│   ├── IRoleRepository.cs     # Интерфейс репозитория ролей
│   └── RoleRepository.cs      # Реализация репозитория ролей
├── Services/                  # Сервисы
│   └── JsonService.cs         # Сервис для работы с JSON
├── Commands/                  # Команды
│   └── RelayCommand.cs        # Базовая реализация ICommand
├── Converters/                # Конвертеры для привязки данных
│   ├── InverseBooleanConverter.cs
│   └── StringToVisibilityConverter.cs
├── MainWindow.xaml            # Главное окно приложения
├── App.xaml                   # Конфигурация приложения
└── appsettings.json           # Конфигурационный файл
```

## Требования

- .NET 8.0 или выше
- PostgreSQL 12 или выше


### Настройка строки подключения

Обновите строку подключения в файле `App.xaml.cs` или установите переменную среды:

```csharp
// В App.xaml.cs
private const string DefaultConnectionString = 
    "Host=localhost;Database=usermanagement;Username=your_username;Password=your_password";
```

Или установите переменную среды:
```bash
set DB_CONNECTION_STRING="Host=localhost;Database=usermanagement;Username=your_username;Password=your_password"
```

### Сборка и запуск

```bash
dotnet restore
dotnet build
dotnet run
```

## Использование

### Главное окно

- **Таблица пользователей**: Отображает всех пользователей с информацией о ролях
- **Кнопка "Обновить"**: Перезагружает список пользователей
- **Кнопка "Добавить"**: Открывает окно добавления нового пользователя
- **Кнопка "Удалить"**: Удаляет выбранного пользователя
- **Кнопка "Экспорт в JSON"**: Сохраняет список пользователей в JSON файл
- **Кнопка "Импорт из JSON"**: Загружает пользователей из JSON файла

### Формат JSON для импорта/экспорта

```json
[
  {
    "Id": 1,
    "Username": "user1",
    "Email": "user1@example.com",
    "Role": "Администратор"
  },
  {
    "Id": 2,
    "Username": "user2",
    "Email": "user2@example.com",
    "Role": "Пользователь"
  }
]
```

### База данных

#### Таблица users
- `user_id` (SERIAL PRIMARY KEY)
- `username` (VARCHAR(50) NOT NULL UNIQUE)
- `email` (VARCHAR(100) NOT NULL UNIQUE)
- `password_hash` (VARCHAR(255) NOT NULL)
- `role_id` (INTEGER NOT NULL)
- `created_at` (TIMESTAMP WITH TIME ZONE)
- `updated_at` (TIMESTAMP WITH TIME ZONE)
- `last_login` (TIMESTAMP WITH TIME ZONE)

#### Таблица roles
- `role_id` (SERIAL PRIMARY KEY)
- `name` (VARCHAR(50) NOT NULL UNIQUE)
- `permissions` (TEXT)

