using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using UserManagementApp.Commands;
using UserManagementApp.Models;
using UserManagementApp.Repositories;
using UserManagementApp.Services;

namespace UserManagementApp.ViewModels
{
    /// <summary>
    /// Главная ViewModel для управления пользователями
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IJsonService _jsonService;

        private User? _selectedUser;
        private bool _isLoading;
        private string _statusMessage = "";

        public MainViewModel(IUserRepository userRepository, IRoleRepository roleRepository, IJsonService jsonService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _jsonService = jsonService ?? throw new ArgumentNullException(nameof(jsonService));

            Users = new ObservableCollection<User>();
            Roles = new ObservableCollection<Role>();

            // Инициализация команд
            LoadUsersCommand = new RelayCommand(async () => await LoadUsersAsync());
            AddUserCommand = new RelayCommand(AddUser);
            DeleteUserCommand = new RelayCommand(async () => await DeleteUserAsync(), () => SelectedUser != null);
            ExportToJsonCommand = new RelayCommand(async () => await ExportToJsonAsync());
            ImportFromJsonCommand = new RelayCommand(async () => await ImportFromJsonAsync());

            // Загружаем данные при инициализации
            _ = Task.Run(InitializeAsync);
        }

        public ObservableCollection<User> Users { get; }
        public ObservableCollection<Role> Roles { get; }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    ((RelayCommand)DeleteUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ExportToJsonCommand { get; }
        public ICommand ImportFromJsonCommand { get; }

        private async Task InitializeAsync()
        {
            try
            {
                await LoadRolesAsync();
                await LoadUsersAsync();
                StatusMessage = "Готово к работе";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка инициализации: {ex.Message}";
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Загрузка пользователей...";

                var users = await _userRepository.GetAllUsersWithRolesAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Users.Clear();
                    foreach (var user in users)
                    {
                        Users.Add(user);
                    }
                });

                StatusMessage = $"Загружено пользователей: {Users.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки пользователей: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _roleRepository.GetAllRolesAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Roles.Clear();
                    foreach (var role in roles)
                    {
                        Roles.Add(role);
                    }
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки ролей: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUser()
        {
            var addUserViewModel = new AddUserViewModel(_userRepository, Roles);
            var addUserWindow = new Views.AddUserWindow
            {
                DataContext = addUserViewModel,
                Owner = Application.Current.MainWindow
            };

            if (addUserWindow.ShowDialog() == true)
            {
                _ = Task.Run(LoadUsersAsync);
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя '{SelectedUser.Username}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Удаление пользователя...";

                var success = await _userRepository.DeleteUserAsync(SelectedUser.UserId);
                if (success)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Users.Remove(SelectedUser);
                        SelectedUser = null;
                    });
                    StatusMessage = "Пользователь успешно удален";
                }
                else
                {
                    StatusMessage = "Не удалось удалить пользователя";
                    MessageBox.Show("Не удалось удалить пользователя", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления: {ex.Message}";
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToJsonAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"users_export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Экспорт в JSON...";

                    await _jsonService.ExportUsersToJsonAsync(Users, saveFileDialog.FileName);
                    StatusMessage = "Экспорт завершен успешно";
                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{saveFileDialog.FileName}", 
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ImportFromJsonAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Импорт из JSON...";

                    var importedUsers = await _jsonService.ImportUsersFromJsonAsync(openFileDialog.FileName);
                    int addedCount = 0;

                    foreach (var importedUser in importedUsers)
                    {
                        // Проверяем, не существует ли уже такой пользователь
                        var exists = await _userRepository.UserExistsAsync(importedUser.Username, importedUser.Email);
                        if (!exists)
                        {
                            // Находим роль по названию
                            var role = await _roleRepository.GetRoleByNameAsync(importedUser.Role);
                            if (role != null)
                            {
                                var user = new User
                                {
                                    Username = importedUser.Username,
                                    Email = importedUser.Email,
                                    RoleId = role.RoleId,
                                    PasswordHash = "", // Устанавливаем пустой пароль
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                await _userRepository.AddUserAsync(user);
                                addedCount++;
                            }
                        }
                    }

                    await LoadUsersAsync();
                    StatusMessage = $"Импорт завершен. Добавлено пользователей: {addedCount}";
                    MessageBox.Show($"Импорт завершен.\nДобавлено пользователей: {addedCount}", 
                        "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка импорта: {ex.Message}";
                MessageBox.Show($"Ошибка при импорте данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
