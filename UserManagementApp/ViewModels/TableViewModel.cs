using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UserManagementApp.Commands;
using UserManagementApp.Models;
using UserManagementApp.Repositories;

namespace UserManagementApp.ViewModels
{
    /// <summary>
    /// ViewModel для работы с произвольными таблицами базы данных
    /// </summary>
    public class TableViewModel : BaseViewModel
    {
        private readonly ITableRepository _tableRepository;
        private DatabaseTable? _selectedTable;
        private DataTable? _tableData;
        private bool _isLoading;
        private string _statusMessage = "";
        private DataRowView? _selectedRow;
        private List<string> _primaryKeyColumns = new();
        private Dictionary<DataRowView, Dictionary<string, object>> _originalValues = new();

        public TableViewModel(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));

            Tables = new ObservableCollection<DatabaseTable>();
            
            // Инициализация пустой DataTable для предотвращения ошибок привязки
            _tableData = new DataTable();

            // Инициализация команд
            LoadTablesCommand = new RelayCommand(async () => await LoadTablesAsync());
            RefreshDataCommand = new RelayCommand(async () => await LoadTableDataAsync(), () => SelectedTable != null);
            AddRowCommand = new RelayCommand(async () => await AddRowAsync(), () => SelectedTable != null);
            DeleteRowCommand = new RelayCommand(async () => await DeleteRowAsync(), () => SelectedTable != null && SelectedRow != null);
            ExportToJsonCommand = new RelayCommand(async () => await ExportToJsonAsync(), () => SelectedTable != null);
            ImportFromJsonCommand = new RelayCommand(async () => await ImportFromJsonAsync(), () => SelectedTable != null);
            ClearLogCommand = new RelayCommand(() => ClearLog());

            // Загружаем список таблиц при инициализации (в фоновом режиме)
            Task.Run(async () =>
            {
                try
                {
                    await LoadTablesAsync();
                }
                catch
                {
                    // Игнорируем ошибки при инициализации, пользователь может загрузить вручную
                }
            });
        }

        public ObservableCollection<DatabaseTable> Tables { get; }

        public DatabaseTable? SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    OnSelectedTableChanged();
                    ((RelayCommand)RefreshDataCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)AddRowCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteRowCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ExportToJsonCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ImportFromJsonCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public DataTable? TableData
        {
            get => _tableData;
            set 
            {
                if (SetProperty(ref _tableData, value))
                {
                    // Уведомляем об изменении TableDataView при изменении TableData
                    OnPropertyChanged(nameof(TableDataView));
                }
            }
        }

        /// <summary>
        /// Свойство для безопасной привязки к DataGrid, всегда возвращает DataView
        /// </summary>
        public DataView TableDataView
        {
            get
            {
                if (_tableData != null)
                {
                    return _tableData.DefaultView;
                }
                
                // Возвращаем пустой DataView если _tableData равен null
                return new DataTable().DefaultView;
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _transactionLog = string.Empty;
        public string TransactionLog
        {
            get => _transactionLog;
            private set => SetProperty(ref _transactionLog, value);
        }

        private readonly HashSet<string> _tablesWithoutPkWarned = new();
        private bool _suppressWarnings = false;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadTablesCommand { get; }
        public ICommand RefreshDataCommand { get; }
        public ICommand AddRowCommand { get; }
        public ICommand DeleteRowCommand { get; }
        public ICommand ExportToJsonCommand { get; }
        public ICommand ImportFromJsonCommand { get; }
        public ICommand ClearLogCommand { get; }

        public DataRowView? SelectedRow
        {
            get => _selectedRow;
            set 
            {
                if (SetProperty(ref _selectedRow, value))
                {
                    ((RelayCommand)DeleteRowCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private async Task LoadTablesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Загрузка списка таблиц...";

                var tables = await _tableRepository.GetAllTablesAsync();

                // Обновляем коллекцию в UI потоке
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Tables.Clear();
                    foreach (var table in tables)
                    {
                        Tables.Add(table);
                    }
                });

                StatusMessage = $"Найдено таблиц: {Tables.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки таблиц: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnSelectedTableChanged()
        {
            if (SelectedTable != null)
            {
                await LoadTableDataAsync();
            }
            else
            {
                TableData = new DataTable(); // Используем пустую таблицу вместо null
            }
        }

        private async Task LoadTableDataAsync()
        {
            if (SelectedTable == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Загрузка данных из таблицы '{SelectedTable.TableName}'...";

                var data = await _tableRepository.GetTableDataAsync(SelectedTable.TableName);
                TableData = data;

                StatusMessage = $"Загружено записей: {data.Rows.Count} из таблицы '{SelectedTable.TableName}'";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки данных: {ex.Message}";
                TableData = new DataTable(); // Используем пустую таблицу вместо null
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddRowAsync()
        {
            if (SelectedTable == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Добавление новой записи...";

                // Простой пример - создаем форму для ввода данных
                // В реальном приложении здесь должно быть диалоговое окно
                var newRowData = new Dictionary<string, object>();
                
                // Получаем информацию о колонках
                var columnsInfo = await _tableRepository.GetTableColumnsAsync(SelectedTable.TableName);
                foreach (DataRow columnRow in columnsInfo.Rows)
                {
                    var columnName = columnRow["column_name"].ToString();
                    var isNullable = columnRow["is_nullable"].ToString() == "YES";
                    
                    // Простая заглушка - в реальном приложении нужен ввод от пользователя
                    if (!isNullable && columnName != null)
                    {
                        newRowData[columnName] = GetDefaultValueForColumn(columnRow);
                    }
                }

                if (newRowData.Any())
                {
                    var success = await _tableRepository.InsertRowAsync(SelectedTable.TableName, newRowData);
                    if (success)
                    {
                        StatusMessage = "Запись успешно добавлена";
                        await LoadTableDataAsync();
                    }
                    else
                    {
                        StatusMessage = "Ошибка при добавлении записи";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка добавления записи: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteRowAsync()
        {
            if (SelectedTable == null || SelectedRow == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Удаление записи...";

                // Получаем первичные ключи
                var primaryKeys = await _tableRepository.GetPrimaryKeyColumnsAsync(SelectedTable.TableName);
                if (!primaryKeys.Any())
                {
                    StatusMessage = "Невозможно удалить запись: не найдены первичные ключи";
                    return;
                }

                var primaryKeyValues = new Dictionary<string, object>();
                foreach (var key in primaryKeys)
                {
                    primaryKeyValues[key] = SelectedRow[key];
                }

                var success = await _tableRepository.DeleteRowAsync(SelectedTable.TableName, primaryKeyValues);
                if (success)
                {
                    StatusMessage = "Запись успешно удалена";
                    await LoadTableDataAsync();
                    SelectedRow = null;
                }
                else
                {
                    StatusMessage = "Ошибка при удалении записи";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка удаления записи: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToJsonAsync()
        {
            if (SelectedTable == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Экспорт данных в JSON...";

                var jsonData = await _tableRepository.ExportTableToJsonAsync(SelectedTable.TableName);
                
                // Сохраняем в файл (упрощенная версия)
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"{SelectedTable.TableName}_export.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllTextAsync(saveFileDialog.FileName, jsonData, System.Text.Encoding.UTF8);
                    StatusMessage = $"Данные экспортированы в файл: {saveFileDialog.FileName}";
                }
                else
                {
                    StatusMessage = "Экспорт отменен";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ImportFromJsonAsync()
        {
            if (SelectedTable == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Импорт данных из JSON...";

                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var jsonData = await System.IO.File.ReadAllTextAsync(openFileDialog.FileName, System.Text.Encoding.UTF8);
                    var success = await _tableRepository.ImportTableFromJsonAsync(SelectedTable.TableName, jsonData);
                    
                    if (success)
                    {
                        StatusMessage = $"Данные успешно импортированы из файла: {openFileDialog.FileName}";
                        await LoadTableDataAsync();
                    }
                    else
                    {
                        StatusMessage = "Ошибка при импорте данных";
                    }
                }
                else
                {
                    StatusMessage = "Импорт отменен";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка импорта: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private object GetDefaultValueForColumn(DataRow columnInfo)
        {
            var dataType = columnInfo["data_type"].ToString()?.ToLower();
            
            return dataType switch
            {
                "integer" or "bigint" or "smallint" => 0,
                "numeric" or "decimal" or "real" or "double precision" => 0.0,
                "boolean" => false,
                "date" or "timestamp" or "timestamp with time zone" => DateTime.Now,
                _ => "Новое значение"
            };
        }

        public async void BeginEdit(DataRowView dataRowView)
        {
            if (dataRowView?.Row == null || SelectedTable == null) return;
            
            // Проверяем, нужно ли показать предупреждение о таблице без PK
            if (!_suppressWarnings && !_tablesWithoutPkWarned.Contains(SelectedTable.TableName))
            {
                var primaryKeys = await _tableRepository.GetPrimaryKeyColumnsAsync(SelectedTable.TableName);
                var uniqueColumns = primaryKeys.Any() ? primaryKeys : await _tableRepository.GetUniqueColumnsAsync(SelectedTable.TableName);
                
                if (!uniqueColumns.Any())
                {
                    // Показываем предупреждение пользователю
                    var dialog = new Views.TableWarningDialog(SelectedTable.TableName);
                    var result = dialog.ShowDialog();
                    
                    if (result != true)
                    {
                        LogTransaction($"Редактирование отменено пользователем для таблицы '{SelectedTable.TableName}' (нет ключей)");
                        return; // Пользователь отменил редактирование
                    }
                    
                    // Запоминаем, что для этой таблицы уже показывали предупреждение
                    _tablesWithoutPkWarned.Add(SelectedTable.TableName);
                    LogTransaction($"Пользователь подтвердил редактирование таблицы '{SelectedTable.TableName}' без ключей");
                }
            }
            
            // Сохраняем оригинальные значения
            var originalValues = new Dictionary<string, object>();
            foreach (DataColumn column in dataRowView.Row.Table.Columns)
            {
                originalValues[column.ColumnName] = dataRowView.Row[column];
            }
            
            _originalValues[dataRowView] = originalValues;
        }
        
        public async Task CommitEdit(DataRowView dataRowView)
        {
            if (dataRowView?.Row == null || SelectedTable == null) return;
            
            LogTransaction($"Начинаем сохранение изменений для таблицы '{SelectedTable.TableName}'");
            
            try
            {
                IsLoading = true;
                StatusMessage = "Сохранение изменений...";
                
                // Получаем первичные ключи
                LogTransaction("Получаем список первичных ключей...");
                var primaryKeys = await _tableRepository.GetPrimaryKeyColumnsAsync(SelectedTable.TableName);
                LogTransaction($"Найдено первичных ключей: {primaryKeys.Count} ({string.Join(", ", primaryKeys)})");
                
                // Если нет первичных ключей, попробуем уникальные колонки
                if (!primaryKeys.Any())
                {
                    LogTransaction("Первичные ключи не найдены. Ищем уникальные колонки...");
                    var uniqueColumns = await _tableRepository.GetUniqueColumnsAsync(SelectedTable.TableName);
                    LogTransaction($"Найдено уникальных колонок: {uniqueColumns.Count} ({string.Join(", ", uniqueColumns)})");
                    
                    if (uniqueColumns.Any())
                    {
                        primaryKeys = uniqueColumns;
                        LogTransaction("Используем уникальные колонки в качестве ключей для UPDATE");
                    }
                    else
                    {
                        LogTransaction("Уникальные колонки не найдены. Будем использовать UPDATE по всем колонкам");
                        // Продолжаем выполнение - обработаем это ниже
                    }
                }
                
                // Формируем данные для обновления и первичные ключи
                var updateData = new Dictionary<string, object>();
                var primaryKeyValues = new Dictionary<string, object>();
                
                // Проверяем, есть ли сохраненные оригинальные значения
                if (!_originalValues.ContainsKey(dataRowView))
                {
                    var errorMsg = "Ошибка: не найдены оригинальные значения для сравнения";
                    StatusMessage = errorMsg;
                    LogTransaction($"ОШИБКА: {errorMsg}");
                    return;
                }
                
                var originalValues = _originalValues[dataRowView];
                bool hasChanges = false;
                
                foreach (DataColumn column in dataRowView.Row.Table.Columns)
                {
                    var columnName = column.ColumnName;
                    var currentValue = dataRowView.Row[column];
                    
                    if (primaryKeys.Contains(columnName))
                    {
                        // Для первичных ключей используем оригинальное значение
                        primaryKeyValues[columnName] = originalValues.ContainsKey(columnName) 
                            ? originalValues[columnName] 
                            : currentValue;
                    }
                    else
                    {
                        // Для остальных колонок проверяем изменения
                        var originalValue = originalValues.ContainsKey(columnName) 
                            ? originalValues[columnName] 
                            : DBNull.Value;
                            
                        // Сравниваем значения
                        if (!Equals(currentValue, originalValue))
                        {
                            updateData[columnName] = currentValue == DBNull.Value ? DBNull.Value : currentValue;
                            hasChanges = true;
                            LogTransaction($"Изменено поле '{columnName}': '{originalValue}' -> '{currentValue}'");
                        }
                    }
                }
                
                LogTransaction($"Анализ изменений завершен. Изменено полей: {updateData.Count}");
                
                if (!hasChanges)
                {
                    var msg = "Изменения не обнаружены";
                    StatusMessage = msg;
                    LogTransaction(msg);
                    _originalValues.Remove(dataRowView);
                    return;
                }
                
                // Выбираем стратегию UPDATE в зависимости от наличия ключей
                bool success = false;
                
                if (primaryKeyValues.Any())
                {
                    // Стандартный UPDATE с первичными/уникальными ключами
                    LogTransaction($"Подготавливаем UPDATE запрос для таблицы '{SelectedTable.TableName}'");
                    LogTransaction($"WHERE условия: {string.Join(", ", primaryKeyValues.Select(kv => $"{kv.Key}={kv.Value}"))}");
                    LogTransaction($"SET значения: {string.Join(", ", updateData.Select(kv => $"{kv.Key}={kv.Value}"))}");
                    
                    LogTransaction("Выполняем UPDATE запрос с ключами в базе данных...");
                    success = await _tableRepository.UpdateRowAsync(SelectedTable.TableName, updateData, primaryKeyValues);
                }
                else
                {
                    // UPDATE по всем колонкам для таблиц без ключей
                    LogTransaction("⚠️ ВНИМАНИЕ: Выполняем UPDATE по всем колонкам (таблица без ключей)");
                    LogTransaction("Формируем полные данные строки для UPDATE...");
                    
                    // Собираем все данные текущей строки
                    var allCurrentData = new Dictionary<string, object>();
                    foreach (DataColumn column in dataRowView.Row.Table.Columns)
                    {
                        allCurrentData[column.ColumnName] = dataRowView.Row[column];
                    }
                    
                    LogTransaction($"Текущие данные строки: {string.Join(", ", allCurrentData.Select(kv => $"{kv.Key}={kv.Value}"))}");
                    LogTransaction($"Оригинальные данные: {string.Join(", ", originalValues.Select(kv => $"{kv.Key}={kv.Value}"))}");
                    
                    success = await _tableRepository.UpdateRowByAllColumnsAsync(SelectedTable.TableName, allCurrentData, originalValues);
                }
                
                // Обрабатываем результат выполнения UPDATE
                if (success)
                {
                    var successMsg = $"Изменения успешно сохранены ({updateData.Count} полей обновлено)";
                    StatusMessage = successMsg;
                    LogTransaction($"✓ УСПЕШНО: {successMsg}");
                    // Очищаем сохраненные оригинальные значения
                    _originalValues.Remove(dataRowView);
                }
                else
                {
                    var errorMsg = "Ошибка при сохранении изменений в базе данных";
                    StatusMessage = errorMsg;
                    LogTransaction($"✗ ОШИБКА: {errorMsg}");
                    // Откатываем изменения
                    CancelEdit(dataRowView);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Ошибка сохранения: {ex.Message}";
                StatusMessage = errorMsg;
                LogTransaction($"✗ ИСКЛЮЧЕНИЕ: {errorMsg}");
                LogTransaction($"Stack trace: {ex.StackTrace}");
                // Откатываем изменения при ошибке
                CancelEdit(dataRowView);
            }
            finally
            {
                IsLoading = false;
                LogTransaction("Завершение операции сохранения");
            }
        }
        
        public void CancelEdit(DataRowView dataRowView)
        {
            if (dataRowView?.Row == null) return;
            
            // Восстанавливаем оригинальные значения
            if (_originalValues.ContainsKey(dataRowView))
            {
                var originalValues = _originalValues[dataRowView];
                foreach (var kvp in originalValues)
                {
                    if (dataRowView.Row.Table.Columns.Contains(kvp.Key))
                    {
                        dataRowView.Row[kvp.Key] = kvp.Value;
                    }
                }
                
                _originalValues.Remove(dataRowView);
            }
        }
        
        #region Методы логирования
        
        private void LogTransaction(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";
            
            if (string.IsNullOrEmpty(TransactionLog))
            {
                TransactionLog = logEntry;
            }
            else
            {
                TransactionLog += Environment.NewLine + logEntry;
            }
        }
        
        private void ClearLog()
        {
            TransactionLog = string.Empty;
        }
        
        #endregion
    }
}
