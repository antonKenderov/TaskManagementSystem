using System.Windows;
using System.Windows.Controls;
using TaskManagementSystem.ViewModels;

namespace TaskManagementSystem.Views
{
    /// <summary>
    /// Interaction logic for TasksView.xaml
    /// </summary>
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TasksViewModel viewModel &&
                viewModel.LoadTasksCommand.CanExecute(null))
            {
                viewModel.LoadTasksCommand.Execute(null);
            }
        }
    }
}
