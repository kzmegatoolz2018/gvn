using System;
using System.Data;
using System.Globalization;
using System.Windows.Data;

namespace UserManagementApp.Converters
{
    /// <summary>
    /// Конвертер для безопасного преобразования DataTable в DataView
    /// </summary>
    public class DataTableToDataViewConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DataTable dataTable)
            {
                return dataTable.DefaultView;
            }
            
            // Возвращаем пустой DataView если значение null или не DataTable
            return new DataTable().DefaultView;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack не поддерживается для DataTableToDataViewConverter");
        }
    }
}
