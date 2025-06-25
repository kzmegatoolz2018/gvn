using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UserManagementApp.Models
{
    /// <summary>
    /// Представляет роль пользователя в системе
    /// </summary>
    public class Role : INotifyPropertyChanged
    {
        private int _roleId;
        private string _name = "";
        private string _permissions = "";

        public int RoleId
        {
            get => _roleId;
            set
            {
                if (_roleId != value)
                {
                    _roleId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Permissions
        {
            get => _permissions;
            set
            {
                if (_permissions != value)
                {
                    _permissions = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
