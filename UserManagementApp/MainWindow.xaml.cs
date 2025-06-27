using System.Windows;
using System.Windows.Controls;
using System.Data;
using UserManagementApp.ViewModels;
using System.ComponentModel;

namespace UserManagementApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(TableViewModel tableViewModel) : this()
    {
        DataContext = tableViewModel;
        
        // Подписываемся на изменения лога для автопрокрутки
        if (tableViewModel != null)
        {
            tableViewModel.PropertyChanged += TableViewModel_PropertyChanged;
        }
    }
    
    private void TableViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TableViewModel.TransactionLog))
        {
            // Прокручиваем лог вниз при добавлении новых записей
            Dispatcher.BeginInvoke(() =>
            {
                var autoScrollCheckBox = FindName("AutoScrollCheckBox") as CheckBox;
                var logTextBox = FindName("LogTextBox") as TextBox;
                
                if (autoScrollCheckBox?.IsChecked == true && logTextBox != null)
                {
                    logTextBox.ScrollToEnd();
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
    }
    
    private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        // Сохраняем оригинальные значения для возможного отката
        if (e.Row.Item is DataRowView dataRowView && DataContext is TableViewModel tableViewModel)
        {
            tableViewModel.BeginEdit(dataRowView);
        }
    }
    
    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit && 
            e.Row.Item is DataRowView dataRowView && 
            DataContext is TableViewModel tableViewModel)
        {
            // Сначала принимаем изменения в DataRow
            if (e.EditingElement is TextBox textBox && e.Column.Header != null)
            {
                var columnName = e.Column.Header.ToString();
                if (!string.IsNullOrEmpty(columnName) && dataRowView.Row.Table.Columns.Contains(columnName))
                {
                    dataRowView.Row[columnName] = textBox.Text;
                }
            }
            
            // Планируем сохранение изменений после завершения редактирования
            // Используем Normal приоритет вместо Background для более быстрого выполнения
            Dispatcher.BeginInvoke(new System.Action(async () =>
            {
                await tableViewModel.CommitEdit(dataRowView);
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }
        else if (e.EditAction == DataGridEditAction.Cancel && 
                 e.Row.Item is DataRowView dataRowView2 && 
                 DataContext is TableViewModel tableViewModel2)
        {
            tableViewModel2.CancelEdit(dataRowView2);
        }
    }
}