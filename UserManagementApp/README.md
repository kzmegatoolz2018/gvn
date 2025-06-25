# Приложение управления пользователями

Настольное WPF приложение на C# для управления пользователями с интеграцией PostgreSQL базы данных.

## Возможности

- **Просмотр пользователей**: Отображение списка всех пользователей с их ролями
- **Добавление пользователей**: Создание новых пользователей с выбором роли
- **Удаление пользователей**: Удаление выбранного пользователя
- **Экспорт в JSON**: Выгрузка списка пользователей в JSON файл
- **Импорт из JSON**: Загрузка пользователей из JSON файла

## Архитектура

Приложение построено с использованием следующих паттернов и принципов:

- **MVVM (Model-View-ViewModel)**: Разделение логики представления от бизнес-логики
- **Repository Pattern**: Абстракция для работы с данными
- **Command Pattern**: Для обработки пользовательских действий
- **SOLID Principles**: Соблюдение принципов объектно-ориентированного программирования
- **Dependency Injection**: Внедрение зависимостей для слабой связанности

## Структура проекта

```
UserManagementApp/
├── Models/                     # Модели данных
│   ├── User.cs                # Модель пользователя
│   ├── Role.cs                # Модель роли
│   └── UserExportModel.cs     # Модель для экспорта/импорта JSON
├── ViewModels/                # ViewModels для MVVM
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
├── Commands/                  # Команды для MVVM
│   └── RelayCommand.cs        # Базовая реализация ICommand
├── Converters/                # Конвертеры для привязки данных
│   ├── InverseBooleanConverter.cs
│   └── StringToVisibilityConverter.cs
├── MainWindow.xaml            # Главное окно приложения
├── App.xaml                   # Конфигурация приложения
├── database_setup.sql         # SQL скрипт для создания БД
└── appsettings.json          # Конфигурационный файл
```

## Требования

- .NET 8.0 или выше
- PostgreSQL 12 или выше
- Visual Studio 2022 или Visual Studio Code

## Установка и настройка

### 1. Клонирование репозитория

```bash
git clone <repository-url>
cd UserManagementApp
```

### 2. Настройка PostgreSQL

1. Установите PostgreSQL на вашем компьютере
2. Создайте новую базу данных:

```sql
CREATE DATABASE usermanagement;
```

3. Выполните SQL скрипт для создания таблиц:

```bash
psql -d usermanagement -f database_setup.sql
```

### 3. Настройка строки подключения

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

### 4. Сборка и запуск

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

## Технические особенности

### Паттерны проектирования

1. **Repository Pattern**: Обеспечивает абстракцию для работы с данными
2. **Command Pattern**: Используется для обработки пользовательских команд
3. **MVVM Pattern**: Разделяет логику представления и бизнес-логику

### Принципы SOLID

- **SRP**: Каждый класс имеет одну ответственность
- **OCP**: Классы открыты для расширения, закрыты для модификации
- **LSP**: Производные классы должны быть заменяемы базовыми
- **ISP**: Интерфейсы разделены по функциональности
- **DIP**: Зависимость от абстракций, а не от конкретных реализаций

### Обработка ошибок

- Все операции с базой данных обернуты в try-catch блоки
- Пользователю отображаются понятные сообщения об ошибках
- Логирование ошибок в статусной строке

## Статус проекта

✅ **Проект готов к использованию**

- Архитектура MVVM реализована
- Repository Pattern применен
- Принципы SOLID соблюдены
- Все основные функции реализованы
- Проект успешно компилируется

## Последние изменения

- Исправлены все ошибки компиляции
- Добавлены конвертеры для привязки данных
- Реализована валидация входных данных
- Добавлена обработка ошибок
- Создана структура задач для VS Code

### Добавление новых возможностей

1. **Новая модель**: Добавьте в папку `Models/`
2. **Новый репозиторий**: Создайте интерфейс и реализацию в `Repositories/`
3. **Новый ViewModel**: Унаследуйте от `BaseViewModel` в `ViewModels/`
4. **Новое окно**: Добавьте XAML и code-behind в `Views/`

### Тестирование

Для тестирования создайте тестовые данные в PostgreSQL:

```sql
INSERT INTO roles (name, permissions) VALUES 
    ('Тестовая роль', 'test_permissions');

INSERT INTO users (username, email, password_hash, role_id) VALUES 
    ('testuser', 'test@example.com', '', 1);
```

## Устранение неполадок

### Проблемы с подключением к БД

1. Проверьте правильность строки подключения
2. Убедитесь, что PostgreSQL сервер запущен
3. Проверьте права доступа пользователя к базе данных

### Проблемы с компиляцией

1. Убедитесь, что установлен .NET 8.0 SDK
2. Выполните `dotnet restore` для восстановления пакетов
3. Проверьте версии NuGet пакетов

## Лицензия

MIT License

## Авторы

Создано с использованием современных подходов разработки WPF приложений с соблюдением принципов Clean Architecture и SOLID.
