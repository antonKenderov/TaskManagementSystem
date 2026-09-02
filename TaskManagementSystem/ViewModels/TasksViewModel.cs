using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;

namespace TaskManagementSystem.ViewModels
{
    public partial class TasksViewModel : ObservableObject
    {
        private readonly ITaskService _taskService;

        public TasksViewModel(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public ObservableCollection<TaskTableItemDto> Tasks { get; } = new();

        [ObservableProperty]
        private TaskTableItemDto? _selectedTask;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoadTasksCommand))]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        private bool CanLoadTasks => !IsLoading;

        [RelayCommand(CanExecute = nameof(CanLoadTasks))]
        private async Task LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var data = await _taskService.GetAllAsync();

                Tasks.Clear();
                foreach (var item in data)
                {
                    Tasks.Add(item);
                }
            }
            catch (Exception ex)
            {
                Tasks.Clear();
                ErrorMessage = $"Could not load the tasks: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
