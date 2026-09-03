using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskManagementSystem.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private const int TasksTabIndex = 0;

        public MainViewModel(TasksViewModel tasksViewModel, SearchViewModel searchViewModel)
        {
            TasksViewModel = tasksViewModel;
            SearchViewModel = searchViewModel;

            SearchViewModel.OpenTask = OpenTaskFromSearchAsync;
        }

        public TasksViewModel TasksViewModel { get; }

        public SearchViewModel SearchViewModel { get; }

        /// <summary>
        /// Drives the sidebar selection, so other screens can navigate here
        /// (the search results opening their task, for example).
        /// </summary>
        [ObservableProperty]
        private int _selectedTabIndex;

        /// <summary>
        /// Set as a callback rather than letting the search view model depend on
        /// this one, which would make the two constructors circular.
        /// </summary>
        private async Task OpenTaskFromSearchAsync(int taskId)
        {
            await TasksViewModel.OpenTaskAsync(taskId);
            SelectedTabIndex = TasksTabIndex;
        }
    }
}
