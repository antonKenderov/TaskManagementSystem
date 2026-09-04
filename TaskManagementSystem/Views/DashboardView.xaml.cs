using System.Windows;
using System.Windows.Controls;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel viewModel &&
                viewModel.LoadCommand.CanExecute(null))
            {
                viewModel.LoadCommand.Execute(null);
            }
        }
    }
}
