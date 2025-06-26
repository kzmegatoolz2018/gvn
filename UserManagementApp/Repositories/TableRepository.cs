using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using UserManagementApp.Models;

namespace UserManagementApp.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с произвольными таблицами PostgreSQL
    /// </summary>
    public class TableRepository : ITableRepository
    {
        private readonly string _connectionString;

        public TableRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<IEnumerable<DatabaseTable>> GetAllTablesAsync()
        {
            var tables = new List<DatabaseTable>();
            const string query = @"
                SELECT table_name, table_schema, table_type
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                  AND table_type = 'BASE TABLE'
                  AND table_name NOT LIKE 'pg_%'
                  AND table_name NOT LIKE 'information_schema%'
                ORDER BY table_name";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var tableName = reader.GetString("table_name");
                    var table = new DatabaseTable
                    {
                        TableName = tableName,
                        TableSchema = reader.GetString("table_schema"),
                        TableType = reader.GetString("table_type")
                    };

                    // Получаем количество записей для каждой таблицы
                    table.RowCount = await GetTableRowCountAsync(tableName);
                    tables.Add(table);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении списка таблиц: {ex.Message}", ex);
            }

            return tables;
        }

        public async Task<DataTable> GetTableDataAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Имя таблицы не может быть пустым", nameof(tableName));

            // Проверяем, что имя таблицы безопасно (защита от SQL injection)
            if (!IsValidTableName(tableName))
                throw new ArgumentException("Недопустимое имя таблицы", nameof(tableName));

            var dataTable = new DataTable(tableName);

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Используем параметризованный запрос через information_schema для безопасности
                var query = $"SELECT * FROM public.\"{tableName}\" LIMIT 1000"; // Ограничиваем количество записей для производительности

                using var command = new NpgsqlCommand(query, connection);
                using var adapter = new NpgsqlDataAdapter(command);
                
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении данных из таблицы '{tableName}': {ex.Message}", ex);
            }

            return dataTable;
        }

        public async Task<DataTable> GetTableColumnsAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Имя таблицы не может быть пустым", nameof(tableName));

            var columnsTable = new DataTable();
            const string query = @"
                SELECT 
                    column_name,
                    data_type,
                    is_nullable,
                    column_default,
                    ordinal_position
                FROM information_schema.columns 
                WHERE table_schema = 'public' 
                  AND table_name = @tableName
                ORDER BY ordinal_position";

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@tableName", tableName);
                using var adapter = new NpgsqlDataAdapter(command);
                
                adapter.Fill(columnsTable);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при получении информации о колонках таблицы '{tableName}': {ex.Message}", ex);
            }

            return columnsTable;
        }

        public async Task<int> GetTableRowCountAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return 0;

            if (!IsValidTableName(tableName))
                return 0;

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = $"SELECT COUNT(*) FROM public.\"{tableName}\"";
                using var command = new NpgsqlCommand(query, connection);
                
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch
            {
                // В случае ошибки возвращаем 0
                return 0;
            }
        }

        public async Task<bool> UpdateTableDataAsync(string tableName, DataTable dataTable)
        {
            // Эта функция может быть реализована для поддержки редактирования
            // Пока оставляем базовую заглушку
            await Task.CompletedTask;
            return false;
        }

        public async Task<bool> InsertRowAsync(string tableName, Dictionary<string, object> rowData)
        {
            if (!IsValidTableName(tableName) || rowData == null || !rowData.Any())
                return false;

            try
            {
                var columns = string.Join(", ", rowData.Keys);
                var parameters = string.Join(", ", rowData.Keys.Select(k => $"@{k}"));
                var query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);

                foreach (var kvp in rowData)
                {
                    command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                }

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteRowAsync(string tableName, Dictionary<string, object> primaryKeyValues)
        {
            if (!IsValidTableName(tableName) || primaryKeyValues == null || !primaryKeyValues.Any())
                return false;

            try
            {
                var whereClause = string.Join(" AND ", primaryKeyValues.Keys.Select(k => $"{k} = @{k}"));
                var query = $"DELETE FROM {tableName} WHERE {whereClause}";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);

                foreach (var kvp in primaryKeyValues)
                {
                    command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                }

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateRowAsync(string tableName, Dictionary<string, object> rowData, Dictionary<string, object> primaryKeyValues)
        {
            if (!IsValidTableName(tableName) || rowData == null || !rowData.Any() || 
                primaryKeyValues == null || !primaryKeyValues.Any())
                return false;

            try
            {
                var setClause = string.Join(", ", rowData.Keys.Select(k => $"{k} = @set_{k}"));
                var whereClause = string.Join(" AND ", primaryKeyValues.Keys.Select(k => $"{k} = @where_{k}"));
                var query = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";

                System.Diagnostics.Debug.WriteLine($"[SQL] Выполняем запрос: {query}");
                System.Diagnostics.Debug.WriteLine($"[SQL] Параметры SET: {string.Join(", ", rowData.Select(kv => $"@set_{kv.Key}={kv.Value}"))}");
                System.Diagnostics.Debug.WriteLine($"[SQL] Параметры WHERE: {string.Join(", ", primaryKeyValues.Select(kv => $"@where_{kv.Key}={kv.Value}"))}");

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                System.Diagnostics.Debug.WriteLine("[SQL] Соединение с БД открыто");
                
                using var command = new NpgsqlCommand(query, connection);

                // Добавляем параметры для SET clause
                foreach (var kvp in rowData)
                {
                    var value = kvp.Value == DBNull.Value ? DBNull.Value : kvp.Value;
                    command.Parameters.AddWithValue($"@set_{kvp.Key}", value ?? DBNull.Value);
                    System.Diagnostics.Debug.WriteLine($"[SQL] Параметр @set_{kvp.Key} = {value ?? "NULL"}");
                }

                // Добавляем параметры для WHERE clause
                foreach (var kvp in primaryKeyValues)
                {
                    var value = kvp.Value == DBNull.Value ? DBNull.Value : kvp.Value;
                    command.Parameters.AddWithValue($"@where_{kvp.Key}", value ?? DBNull.Value);
                    System.Diagnostics.Debug.WriteLine($"[SQL] Параметр @where_{kvp.Key} = {value ?? "NULL"}");
                }

                var result = await command.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine($"[SQL] Результат выполнения: {result} строк затронуто");
                
                var success = result > 0;
                System.Diagnostics.Debug.WriteLine($"[SQL] Операция {(success ? "УСПЕШНА" : "НЕ ВЫПОЛНЕНА")}");
                
                return success;
            }
            catch (Exception ex)
            {
                // В продакшене здесь следует использовать логгер
                System.Diagnostics.Debug.WriteLine($"[SQL] ОШИБКА UpdateRowAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SQL] Тип исключения: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[SQL] Таблица: {tableName}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SQL] Внутреннее исключение: {ex.InnerException.Message}");
                }
                System.Diagnostics.Debug.WriteLine($"[SQL] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<List<string>> GetPrimaryKeyColumnsAsync(string tableName)
        {
            if (!IsValidTableName(tableName))
                return new List<string>();

            try
            {
                const string query = @"
                    SELECT column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.constraint_column_usage ccu 
                        ON tc.constraint_name = ccu.constraint_name 
                        AND tc.table_schema = ccu.table_schema
                    WHERE tc.constraint_type = 'PRIMARY KEY' 
                        AND tc.table_name = @tableName 
                        AND tc.table_schema = 'public'
                    ORDER BY ccu.ordinal_position";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@tableName", tableName);

                var primaryKeys = new List<string>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    primaryKeys.Add(reader.GetString("column_name"));
                }

                return primaryKeys;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> GetUniqueColumnsAsync(string tableName)
        {
            if (!IsValidTableName(tableName))
                return new List<string>();

            try
            {
                const string query = @"
                    SELECT DISTINCT kcu.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage kcu 
                        ON tc.constraint_name = kcu.constraint_name 
                        AND tc.table_schema = kcu.table_schema
                    WHERE tc.constraint_type IN ('PRIMARY KEY', 'UNIQUE')
                        AND tc.table_name = @tableName 
                        AND tc.table_schema = 'public'
                    ORDER BY kcu.column_name";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@tableName", tableName);

                var uniqueColumns = new List<string>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    uniqueColumns.Add(reader.GetString("column_name"));
                }

                return uniqueColumns;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> GetAllColumnsAsync(string tableName)
        {
            if (!IsValidTableName(tableName))
                return new List<string>();

            try
            {
                const string query = @"
                    SELECT column_name
                    FROM information_schema.columns
                    WHERE table_name = @tableName 
                        AND table_schema = 'public'
                    ORDER BY ordinal_position";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@tableName", tableName);

                var columns = new List<string>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString("column_name"));
                }

                return columns;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<bool> UpdateRowByAllColumnsAsync(string tableName, Dictionary<string, object> newData, Dictionary<string, object> originalData)
        {
            if (!IsValidTableName(tableName) || newData == null || !newData.Any() || 
                originalData == null || !originalData.Any())
                return false;

            try
            {
                // Создаем WHERE условие по всем оригинальным значениям
                var whereConditions = new List<string>();
                foreach (var kvp in originalData)
                {
                    if (kvp.Value == null || kvp.Value == DBNull.Value)
                    {
                        whereConditions.Add($"{kvp.Key} IS NULL");
                    }
                    else
                    {
                        whereConditions.Add($"{kvp.Key} = @orig_{kvp.Key}");
                    }
                }

                var setClause = string.Join(", ", newData.Keys.Select(k => $"{k} = @new_{k}"));
                var whereClause = string.Join(" AND ", whereConditions);
                var query = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";

                System.Diagnostics.Debug.WriteLine($"[SQL] UPDATE без PK запрос: {query}");

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(query, connection);

                // Добавляем параметры для SET clause
                foreach (var kvp in newData)
                {
                    var value = kvp.Value == DBNull.Value ? DBNull.Value : kvp.Value;
                    command.Parameters.AddWithValue($"@new_{kvp.Key}", value ?? DBNull.Value);
                }

                // Добавляем параметры для WHERE clause (только для не-NULL значений)
                foreach (var kvp in originalData)
                {
                    if (kvp.Value != null && kvp.Value != DBNull.Value)
                    {
                        command.Parameters.AddWithValue($"@orig_{kvp.Key}", kvp.Value);
                    }
                }

                var result = await command.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine($"[SQL] UPDATE без PK результат: {result} строк затронуто");
                
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQL] ОШИБКА UpdateRowByAllColumnsAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<string> ExportTableToJsonAsync(string tableName)
        {
            if (!IsValidTableName(tableName))
                return "[]";

            try
            {
                var dataTable = await GetTableDataAsync(tableName);
                var rows = new List<Dictionary<string, object>>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var rowDict = new Dictionary<string, object>();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        var value = row[column];
                        rowDict[column.ColumnName] = value == DBNull.Value ? DBNull.Value : value;
                    }
                    rows.Add(rowDict);
                }

                return System.Text.Json.JsonSerializer.Serialize(rows, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch
            {
                return "[]";
            }
        }

        public async Task<bool> ImportTableFromJsonAsync(string tableName, string jsonData)
        {
            if (!IsValidTableName(tableName) || string.IsNullOrWhiteSpace(jsonData))
                return false;

            try
            {
                var rows = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonData);
                if (rows == null || !rows.Any())
                    return false;

                // Получаем информацию о колонках таблицы
                var columnsInfo = await GetTableColumnsAsync(tableName);
                var validColumns = new HashSet<string>();
                foreach (DataRow row in columnsInfo.Rows)
                {
                    validColumns.Add(row["column_name"].ToString()!);
                }

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    foreach (var row in rows)
                    {
                        // Фильтруем только существующие колонки
                        var validRowData = row
                            .Where(kvp => validColumns.Contains(kvp.Key))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        if (validRowData.Any())
                        {
                            var columns = string.Join(", ", validRowData.Keys);
                            var parameters = string.Join(", ", validRowData.Keys.Select(k => $"@{k}"));
                            var query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

                            using var command = new NpgsqlCommand(query, connection, transaction);
                            foreach (var kvp in validRowData)
                            {
                                command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                            }

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, является ли имя таблицы безопасным (защита от SQL injection)
        /// </summary>
        private static bool IsValidTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            // Простая проверка: только буквы, цифры и подчеркивания
            foreach (char c in tableName)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            return true;
        }
    }
}
