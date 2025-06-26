using System.Windows;

namespace UserManagementApp.Views
{
    public partial class TableWarningDialog : Window
    {
        public string TableName { get; set; } = string.Empty;
        public bool ContinueEditing { get; private set; }

        public TableWarningDialog(string tableName)
        {
            InitializeComponent();
            TableName = tableName;
            DataContext = this;
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            ContinueEditing = true;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ContinueEditing = false;
            DialogResult = false;
        }
    }
}
