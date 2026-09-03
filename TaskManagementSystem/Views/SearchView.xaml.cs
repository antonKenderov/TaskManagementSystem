using System.Windows;
using System.Windows.Controls;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem.Views
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SearchViewModel viewModel &&
                viewModel.SearchCommand.CanExecute(null))
            {
                viewModel.SearchCommand.Execute(null);
            }
        }
    }
}
