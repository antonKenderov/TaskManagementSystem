using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskManagementSystem.Application.DTOs;
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

        /// <summary>
        /// Opens the clicked task. A MouseBinding on the grid runs before the
        /// DataGrid has updated its selection, and the bubbling MouseLeftButtonUp is
        /// swallowed by DataGridCell, so the row listens on the preview instead: by
        /// then the mouse-down has already moved the selection here.
        /// </summary>
        private void OnRowClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow { DataContext: TaskTableItemDto task } &&
                DataContext is TasksViewModel viewModel)
            {
                viewModel.SelectedTask = task;

                if (viewModel.OpenTaskDetailCommand.CanExecute(null))
                {
                    viewModel.OpenTaskDetailCommand.Execute(null);
                }
            }
        }
    }
}
