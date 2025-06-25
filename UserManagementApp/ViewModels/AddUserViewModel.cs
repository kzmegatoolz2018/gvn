using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UserManagementApp.Commands;
using UserManagementApp.Models;
using UserManagementApp.Repositories;

namespace UserManagementApp.ViewModels
{
    /// <summary>
    /// ViewModel для добавления нового пользователя
    /// </summary>
    public class AddUserViewModel : BaseViewModel
    {
        private readonly IUserRepository _userRepository;
        private string _username = "";
        private string _email = "";
        private Role? _selectedRole;
        private bool _isLoading;
        private string _validationMessage = "";

        public AddUserViewModel(IUserRepository userRepository, ObservableCollection<Role> roles)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));

            SaveCommand = new RelayCommand(async () => await SaveUserAsync(), CanSaveUser);
            CancelCommand = new RelayCommand(Cancel);
        }

        public ObservableCollection<Role> Roles { get; }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ValidateInput();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateInput();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    ValidateInput();
                    ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set => SetProperty(ref _validationMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        private bool CanSaveUser()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   SelectedRole != null &&
                   !IsLoading &&
                   string.IsNullOrEmpty(ValidationMessage);
        }

        private void ValidateInput()
        {
            ValidationMessage = "";

            if (string.IsNullOrWhiteSpace(Username))
            {
                ValidationMessage = "Имя пользователя обязательно для заполнения";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ValidationMessage = "Email обязателен для заполнения";
                return;
            }

            if (!IsValidEmail(Email))
            {
                ValidationMessage = "Введите корректный email адрес";
                return;
            }

            if (SelectedRole == null)
            {
                ValidationMessage = "Выберите роль пользователя";
                return;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task SaveUserAsync()
        {
            try
            {
                IsLoading = true;
                ValidationMessage = "";

                // Проверяем, не существует ли уже такой пользователь
                var exists = await _userRepository.UserExistsAsync(Username, Email);
                if (exists)
                {
                    ValidationMessage = "Пользователь с таким именем или email уже существует";
                    return;
                }

                var user = new User
                {
                    Username = Username.Trim(),
                    Email = Email.Trim(),
                    RoleId = SelectedRole!.RoleId,
                    PasswordHash = "", // Оставляем пустым, т.к. аутентификация не требуется
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var userId = await _userRepository.AddUserAsync(user);
                
                if (userId > 0)
                {
                    MessageBox.Show("Пользователь успешно добавлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestClose?.Invoke(this, true);
                }
                else
                {
                    ValidationMessage = "Не удалось добавить пользователя";
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Ошибка при добавлении пользователя: {ex.Message}";
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }
    }
}
