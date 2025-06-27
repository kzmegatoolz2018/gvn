using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UserManagementApp.Models;

namespace UserManagementApp.Repositories
{
    /// <summary>
    /// Интерфейс для работы с произвольными таблицами базы данных
    /// </summary>
    public interface ITableRepository
    {
        /// <summary>
        /// Тестирование подключения к базе данных
        /// </summary>
        Task<(bool isConnected, string connectionInfo, string errorMessage)> TestConnectionAsync();

        /// <summary>
        /// Получить список всех пользовательских таблиц в базе данных
        /// </summary>
        Task<IEnumerable<DatabaseTable>> GetAllTablesAsync();

        /// <summary>
        /// Получить все данные из указанной таблицы
        /// </summary>
        Task<DataTable> GetTableDataAsync(string tableName);

        /// <summary>
        /// Получить информацию о колонках таблицы
        /// </summary>
        Task<DataTable> GetTableColumnsAsync(string tableName);

        /// <summary>
        /// Получить количество записей в таблице
        /// </summary>
        Task<int> GetTableRowCountAsync(string tableName);

        /// <summary>
        /// Обновить данные в таблице (опционально)
        /// </summary>
        Task<bool> UpdateTableDataAsync(string tableName, DataTable dataTable);

        /// <summary>
        /// Добавить новую запись в таблицу
        /// </summary>
        Task<bool> InsertRowAsync(string tableName, Dictionary<string, object> rowData);

        /// <summary>
        /// Удалить запись из таблицы по первичному ключу
        /// </summary>
        Task<bool> DeleteRowAsync(string tableName, Dictionary<string, object> primaryKeyValues);

        /// <summary>
        /// Обновить запись в таблице
        /// </summary>
        Task<bool> UpdateRowAsync(string tableName, Dictionary<string, object> rowData, Dictionary<string, object> primaryKeyValues);

        /// <summary>
        /// Обновить строку в таблице по всем колонкам (для таблиц без первичного ключа)
        /// </summary>
        Task<bool> UpdateRowByAllColumnsAsync(string tableName, Dictionary<string, object> newData, Dictionary<string, object> originalData);

        /// <summary>
        /// Получить информацию о первичных ключах таблицы
        /// </summary>
        Task<List<string>> GetPrimaryKeyColumnsAsync(string tableName);

        /// <summary>
        /// Получить список уникальных колонок для указанной таблицы (включая PK и UNIQUE)
        /// </summary>
        Task<List<string>> GetUniqueColumnsAsync(string tableName);

        /// <summary>
        /// Получить список всех колонок таблицы
        /// </summary>
        Task<List<string>> GetAllColumnsAsync(string tableName);

        /// <summary>
        /// Экспортировать данные таблицы в JSON
        /// </summary>
        Task<string> ExportTableToJsonAsync(string tableName);

        /// <summary>
        /// Импортировать данные из JSON в таблицу
        /// </summary>
        Task<bool> ImportTableFromJsonAsync(string tableName, string jsonData);
    }
}
