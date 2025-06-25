using System.Windows;
using UserManagementApp.ViewModels;

namespace UserManagementApp.Views
{
    /// <summary>
    /// Логика взаимодействия для AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        public AddUserWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is AddUserViewModel viewModel)
            {
                viewModel.RequestClose += OnRequestClose;
            }

            if (e.OldValue is AddUserViewModel oldViewModel)
            {
                oldViewModel.RequestClose -= OnRequestClose;
            }
        }

        private void OnRequestClose(object? sender, bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
    }
}
