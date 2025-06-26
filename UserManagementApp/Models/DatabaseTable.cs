using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UserManagementApp.Models
{
    /// <summary>
    /// Представляет информацию о таблице базы данных
    /// </summary>
    public class DatabaseTable : INotifyPropertyChanged
    {
        private string _tableName = "";
        private string _tableSchema = "";
        private string _tableType = "";
        private int _rowCount;

        public string TableName
        {
            get => _tableName;
            set
            {
                if (_tableName != value)
                {
                    _tableName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TableSchema
        {
            get => _tableSchema;
            set
            {
                if (_tableSchema != value)
                {
                    _tableSchema = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TableType
        {
            get => _tableType;
            set
            {
                if (_tableType != value)
                {
                    _tableType = value;
                    OnPropertyChanged();
                }
            }
        }

        public int RowCount
        {
            get => _rowCount;
            set
            {
                if (_rowCount != value)
                {
                    _rowCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayName => $"{TableName} ({RowCount} записей)";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
