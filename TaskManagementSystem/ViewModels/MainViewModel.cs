using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskManagementSystem.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel(TasksViewModel tasksViewModel)
        {
            TasksViewModel = tasksViewModel;
        }

        public TasksViewModel TasksViewModel { get; }

        /// <summary>
        /// Drives the sidebar selection, so other screens can navigate here
        /// (the search results opening their task, for example).
        /// </summary>
        [ObservableProperty]
        private int _selectedTabIndex;
    }
}
